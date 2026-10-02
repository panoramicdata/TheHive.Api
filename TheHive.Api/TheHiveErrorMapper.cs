using System.Text.Json;

namespace TheHive.Api;

internal static class TheHiveErrorMapper
{
	public static async Task<Exception?> CreateAsync(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			return null;
		}

		string? type = null;
		string? message = null;
		var body = await response.Content.ReadAsStringAsync();
		try
		{
			using var document = JsonDocument.Parse(body);
			if (document.RootElement.ValueKind == JsonValueKind.Object)
			{
				type = document.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;
				message = document.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
			}
		}
		catch (JsonException)
		{
			// Non-JSON body (e.g. a proxy error page): fall through to the fallback message.
		}

		var requestId = response.Headers.TryGetValues("X-Request-Id", out var values) ? values.FirstOrDefault() : null;
		return new TheHiveApiException(
			response.StatusCode,
			type,
			message ?? $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase ?? response.StatusCode.ToString()})",
			requestId);
	}
}
