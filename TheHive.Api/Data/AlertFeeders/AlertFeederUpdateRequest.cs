using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>
/// The body of an update-alert-feeder request (the spec's <c>InputUpdateAlertFeeder</c>). Unset properties are omitted; the spec defines
/// no clearable fields. The feeder name and function cannot be changed.
/// </summary>
public sealed class AlertFeederUpdateRequest
{
	/// <summary>The new description (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The new HTTP method.</summary>
	[JsonPropertyName("method")]
	public required AlertFeederMethod Method { get; set; }

	/// <summary>The new endpoint URL of the external system.</summary>
	[JsonPropertyName("url")]
	public required string Url { get; set; }

	/// <summary>The new polling interval; the minimum is 1 minute.</summary>
	[JsonPropertyName("interval")]
	public required Interval Interval { get; set; }

	/// <summary>The new HTTP request body; used when the method is <c>POST</c>.</summary>
	[JsonPropertyName("body")]
	public string? Body { get; set; }

	/// <summary>The new HTTP headers; they replace the current set. Header values can be secrets: they travel only in this request body.</summary>
	[JsonPropertyName("headers")]
	public List<AlertFeederHeader>? Headers { get; set; }

	/// <summary>Whether the alert feeder is active and runs on schedule (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("enabled")]
	public bool? Enabled { get; set; }

	/// <summary>The new authentication configuration (secrets travel only in this request body).</summary>
	[JsonPropertyName("auth")]
	public AlertFeederAuth? Auth { get; set; }

	/// <summary>The new proxy configuration.</summary>
	[JsonPropertyName("proxyConfig")]
	public ClientProxyConfig? ProxyConfig { get; set; }

	/// <summary>The new maximum time to wait for the external system to respond (the server default is 10 seconds).</summary>
	[JsonPropertyName("requestTimeout")]
	public Interval? RequestTimeout { get; set; }

	/// <summary>The new maximum response payload size in bytes (the server default is 10485760); it must be greater than 0.</summary>
	[JsonPropertyName("responseMaxSize")]
	public long? ResponseMaxSize { get; set; }
}
