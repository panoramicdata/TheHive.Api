using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace TheHive.Api.Handlers;

internal sealed class AuthRetryHandler(TheHiveClientOptions options) : DelegatingHandler
{
	internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } = Task.Delay;

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
		if (!string.IsNullOrWhiteSpace(options.Organisation))
		{
			request.Headers.Remove("X-Organisation");
			request.Headers.Add("X-Organisation", options.Organisation);
		}

		var backoff = options.RetryBaseDelay;
		for (var attempt = 0; ; attempt++)
		{
			options.Logger?.LogDebug("TheHive {Method} {Uri} (attempt {Attempt})", request.Method, request.RequestUri, attempt + 1);
			var response = await SendAttemptAsync(request, cancellationToken);
			if (!IsTransient(response.StatusCode) || attempt >= options.MaxRetries)
			{
				return response;
			}

			var wait = RetryAfter(response) ?? backoff;
			options.Logger?.LogWarning("TheHive returned {Status}; retrying in {Delay}", (int)response.StatusCode, wait);
			response.Dispose();
			await Delay(wait, cancellationToken);
			backoff *= 2;
		}
	}

	private async Task<HttpResponseMessage> SendAttemptAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		using var attemptCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		attemptCts.CancelAfter(options.Timeout);
		try
		{
			return await base.SendAsync(request, attemptCts.Token);
		}
		catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attemptCts.IsCancellationRequested)
		{
			throw new TimeoutException($"TheHive did not respond within {options.Timeout}.");
		}
	}

	private static bool IsTransient(HttpStatusCode status) => status == HttpStatusCode.TooManyRequests || (int)status >= 500;

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
