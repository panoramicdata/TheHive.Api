using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>An HTTP header sent with the alert feeder request (the spec's <c>Header</c>).</summary>
public sealed class AlertFeederHeader
{
	/// <summary>The header name (printable ASCII).</summary>
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	/// <summary>The header value (1 to 8192 characters). SECRET when the header carries credentials (for example an API key): it only ever travels in a request or response body.</summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;
}
