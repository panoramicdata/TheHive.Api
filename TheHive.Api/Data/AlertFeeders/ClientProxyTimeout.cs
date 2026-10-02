using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>Connection, idle and request timeouts of the HTTP client (the spec's <c>Timeout</c>). The spec types each as a string (a duration such as <c>10 seconds</c>).</summary>
public sealed class ClientProxyTimeout
{
	/// <summary>The maximum time to wait to establish a connection.</summary>
	[JsonPropertyName("connection")]
	public string? Connection { get; set; }

	/// <summary>The maximum time a connection may remain idle before being closed.</summary>
	[JsonPropertyName("idle")]
	public string? Idle { get; set; }

	/// <summary>The maximum time to wait for a complete response.</summary>
	[JsonPropertyName("request")]
	public string? Request { get; set; }
}
