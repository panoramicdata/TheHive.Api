using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace TheHive.Api.Handlers;

/// <summary>
/// Adds authentication and retries transient failures. A request is retried (up to <see cref="TheHiveClientOptions.MaxRetries"/>
/// times, honouring <c>Retry-After</c>, else exponential back-off) only when its body is replayable (none, string, byte array,
/// form, memory, JSON or Refit [Body] content) and either the status is 429 or 503, or the status is another 5xx and the verb is idempotent
/// (GET, HEAD, PUT, DELETE, OPTIONS, TRACE). Stream and multipart bodies (uploads) are never retried: the first response is
/// returned and maps to <see cref="TheHiveApiException"/>. Each wait (back-off or <c>Retry-After</c>) is capped at <see cref="TheHiveClientOptions.MaxRetryDelay"/>. Only the request path is logged, never the query string. The options are copied at construction.
/// </summary>
internal sealed class AuthRetryHandler : DelegatingHandler
{
	// The options are read once, here: changing the options object later does not affect a client that already exists.
	private readonly string _apiKey;
	private readonly string? _organisation;
	private readonly ILogger? _logger;
	private readonly TimeSpan _timeout;
	private readonly int _maxRetries;
	private readonly TimeSpan _retryBaseDelay;
	private readonly TimeSpan _maxRetryDelay;

	public AuthRetryHandler(TheHiveClientOptions options)
	{
		_apiKey = options.ApiKey;
		_organisation = options.Organisation;
		_logger = options.Logger;
		_timeout = options.Timeout;
		_maxRetries = options.MaxRetries;
		_retryBaseDelay = options.RetryBaseDelay;
		_maxRetryDelay = options.MaxRetryDelay;
	}

	internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } = Task.Delay;

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
		if (!string.IsNullOrWhiteSpace(_organisation))
		{
			request.Headers.Remove("X-Organisation");
			request.Headers.Add("X-Organisation", _organisation);
		}

		// Only the path is logged: query strings can carry secrets (for example the case export password).
		var path = request.RequestUri!.GetLeftPart(UriPartial.Path);
		var replayable = IsReplayable(request.Content);
		var backoff = _retryBaseDelay;
		for (var attempt = 0; ; attempt++)
		{
			_logger?.LogDebug("TheHive {Method} {Path} (attempt {Attempt})", request.Method, path, attempt + 1);
			var response = await SendAttemptAsync(request, cancellationToken);
			if (!replayable || !IsRetryable(request.Method, response.StatusCode) || attempt >= _maxRetries)
			{
				return response;
			}

			var wait = RetryAfter(response) ?? backoff;
			if (wait > _maxRetryDelay)
			{
				wait = _maxRetryDelay;
			}

			_logger?.LogWarning("TheHive returned {Status}; retrying in {Delay}", (int)response.StatusCode, wait);
			response.Dispose();
			await Delay(wait, cancellationToken);
			// Doubling is capped so it can never overflow TimeSpan.
			backoff = backoff > _maxRetryDelay / 2 ? _maxRetryDelay : backoff * 2;
		}
	}

	private async Task<HttpResponseMessage> SendAttemptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		attemptCts.CancelAfter(_timeout);
		try
		{
			return await base.SendAsync(request, attemptCts.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attemptCts.IsCancellationRequested)
		{
			throw new TimeoutException($"TheHive did not respond within {_timeout}.");
		}
	}

	private static readonly HashSet<HttpMethod> IdempotentMethods =
		[HttpMethod.Get, HttpMethod.Head, HttpMethod.Put, HttpMethod.Delete, HttpMethod.Options, HttpMethod.Trace];

	/// <summary>
	/// Whether the body can be sent again unchanged. Buffered content types are; streams, multipart bodies and unknown
	/// content types are not (a stream may be read-once, and a multipart upload may already have been stored).
	/// </summary>
	/// <remarks>
	/// <see cref="RefitBodyContentType"/> is the content Refit builds for <c>[Body]</c> objects (an internal push-stream type that
	/// re-runs the JSON serializer on every send), so it is replayable. Without it, ordinary JSON requests would never be
	/// retried, not even on 429 or 503.
	/// </remarks>
	private static bool IsReplayable(HttpContent? content)
		=> content is null or ByteArrayContent or ReadOnlyMemoryContent or System.Net.Http.Json.JsonContent
			|| content.GetType() == RefitBodyContentType;

	/// <summary>
	/// The runtime type of the content Refit sends for an unbuffered <c>[Body]</c> (its serializer's streaming content),
	/// found by serializing an arbitrary model once, so it follows Refit upgrades.
	/// </summary>
	private static readonly Type RefitBodyContentType = CreateRefitBodyContentType();

	private static Type CreateRefitBodyContentType()
	{
		using var probe = new Refit.SystemTextJsonContentSerializer(TheHiveJson.Options).ToStreamingHttpContent(new Data.Attachments.AttachmentUpdateRequest());
		return probe.GetType();
	}

	/// <summary>
	/// 429 and 503 mean the request was not processed, so any verb is retried. Other 5xx may follow a partial or complete
	/// write, so only idempotent verbs are retried on them (never POST or PATCH).
	/// </summary>
	private static bool IsRetryable(HttpMethod method, HttpStatusCode status)
		=> status is HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable
			|| ((int)status >= 500 && IdempotentMethods.Contains(method));

	private static TimeSpan? RetryAfter(HttpResponseMessage response)
	{
		var header = response.Headers.RetryAfter;
		if (header?.Delta is { } delta)
		{
			return delta;
		}

		if (header?.Date is { } date)
		{
			var wait = date - DateTimeOffset.UtcNow;
			return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
		}

		return null;
	}
}
