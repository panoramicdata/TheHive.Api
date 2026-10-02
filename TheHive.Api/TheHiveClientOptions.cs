using Microsoft.Extensions.Logging;

namespace TheHive.Api;

/// <summary>Configuration for <see cref="TheHiveClient"/>.</summary>
public class TheHiveClientOptions
{
	/// <summary>Absolute URL of the TheHive instance, e.g. <c>https://thehive.example.com</c>.</summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>The API key sent as a bearer token.</summary>
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>Optional organisation name, sent as <c>X-Organisation</c>.</summary>
	public string? Organisation { get; set; }

	/// <summary>
	/// HTTP timeout per attempt, covering sending the request body and receiving the response headers (so large uploads
	/// need a larger value). It does not include retry back-off or <c>Retry-After</c> waits, nor reading a downloaded body
	/// after the headers arrive; pass a cancellation token to <c>ReadAs*Async</c> to bound that.
	/// An attempt that exceeds it raises a <see cref="TimeoutException"/>; caller cancellation still raises <see cref="OperationCanceledException"/>.
	/// </summary>
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

	/// <summary>
	/// Maximum retries of a transient failure. Any verb is retried on 429 and 503; other 5xx responses are retried only for
	/// idempotent verbs (GET, HEAD, PUT, DELETE, OPTIONS, TRACE), never POST or PATCH. Requests with a stream or multipart body
	/// (file uploads, case import) are never retried, so an upload is never sent twice; their first error response is raised as
	/// <see cref="TheHiveApiException"/>.
	/// </summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>Initial back-off, doubled on each retry.</summary>
	public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

	/// <summary>Optional logger. The API key and request query strings (which may hold secrets such as an export password) are never logged.</summary>
	public ILogger? Logger { get; set; }

	internal void Validate()
	{
		if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
		{
			throw new ArgumentException("BaseUrl must be an absolute URL.", nameof(BaseUrl));
		}

		ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);
		ArgumentOutOfRangeException.ThrowIfNegative(MaxRetries);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Timeout, TimeSpan.Zero);
	}

	/// <inheritdoc />
	public override string ToString() => $"TheHiveClientOptions {{ BaseUrl = {BaseUrl}, ApiKey = ***, Organisation = {Organisation} }}";
}
