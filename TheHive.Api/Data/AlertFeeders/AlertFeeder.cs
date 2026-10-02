using System.Text.Json.Serialization;
using TheHive.Api.Data.Functions;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>
/// An alert feeder: a schedule that periodically requests data from an external system over HTTP and converts it into alerts with a
/// feeder function (the spec's <c>OutputAlertFeeder</c>). Credentials in <see cref="Auth"/>, <see cref="Headers"/> and
/// <see cref="ProxyConfig"/> are reported as the server returns them.
/// </summary>
public sealed class AlertFeeder
{
	/// <summary>The name of the alert feeder.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>A description of the alert feeder.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The HTTP method used to request data.</summary>
	[JsonPropertyName("method")]
	public AlertFeederMethod Method { get; set; }

	/// <summary>The endpoint URL of the external system.</summary>
	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	/// <summary>The polling interval.</summary>
	[JsonPropertyName("interval")]
	public Interval Interval { get; set; } = new();

	/// <summary>The feeder function that converts the HTTP response into alerts.</summary>
	[JsonPropertyName("function")]
	public Function Function { get; set; } = new();

	/// <summary>The HTTP headers included in the request.</summary>
	[JsonPropertyName("headers")]
	public List<AlertFeederHeader>? Headers { get; set; }

	/// <summary>The authentication configuration.</summary>
	[JsonPropertyName("auth")]
	public AlertFeederAuth? Auth { get; set; }

	/// <summary>The HTTP request body sent with the request.</summary>
	[JsonPropertyName("body")]
	public string? Body { get; set; }

	/// <summary>Whether the alert feeder is active and runs on schedule.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>The maximum time to wait for the external system to respond.</summary>
	[JsonPropertyName("requestTimeout")]
	public Interval RequestTimeout { get; set; } = new();

	/// <summary>The maximum response payload size, in bytes.</summary>
	[JsonPropertyName("responseMaxSize")]
	public long ResponseMaxSize { get; set; }

	/// <summary>The proxy configuration for the HTTP request.</summary>
	[JsonPropertyName("proxyConfig")]
	public ClientProxyConfig? ProxyConfig { get; set; }
}
