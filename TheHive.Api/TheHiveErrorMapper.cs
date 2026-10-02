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
		var body = await ReadBodyAsync(response);
		try
		{
			using var document = JsonDocument.Parse(body);
			if (document.RootElement.ValueKind == JsonValueKind.Object)
			{
				type = TryGetString(document.RootElement, "type");
				message = TryGetString(document.RootElement, "message");
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

	private static async Task<string> ReadBodyAsync(HttpResponseMessage response)
	{
		try
		{
			return await response.Content.ReadAsStringAsync();
		}
		catch (InvalidOperationException)
		{
			// Unsupported charset in Content-Type: the body is unreadable, use the fallback message.
			return string.Empty;
		}
	}

	private static string? TryGetString(JsonElement element, string name)
		=> element.TryGetProperty(name, out var property) && property.ValueKind == JsonValueKind.String
			? property.GetString()
			: null;
}
