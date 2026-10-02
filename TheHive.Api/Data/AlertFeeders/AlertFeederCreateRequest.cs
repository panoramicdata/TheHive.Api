using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The body of a create-alert-feeder request (the spec's <c>InputAlertFeeder</c>). Unset properties are omitted. The feeder function must already exist and have the type <c>feeder:alert</c>.</summary>
public sealed class AlertFeederCreateRequest
{
	/// <summary>The unique name (1 to 128 characters); it cannot be changed after creation.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>A description of the alert feeder (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The HTTP method used to request data.</summary>
	[JsonPropertyName("method")]
	public required AlertFeederMethod Method { get; set; }

	/// <summary>The endpoint URL of the external system.</summary>
	[JsonPropertyName("url")]
	public required string Url { get; set; }

	/// <summary>The polling interval; the minimum is 1 minute.</summary>
	[JsonPropertyName("interval")]
	public required Interval Interval { get; set; }

	/// <summary>The name of the feeder function that converts the HTTP response into alerts; it must reference an existing function of type <c>feeder:alert</c>.</summary>
	[JsonPropertyName("functionName")]
	public required string FunctionName { get; set; }

	/// <summary>The HTTP request body to send; used when the method is <c>POST</c>.</summary>
	[JsonPropertyName("body")]
	public string? Body { get; set; }

	/// <summary>The HTTP headers to include in the request. Header values can be secrets: they travel only in this request body.</summary>
	[JsonPropertyName("headers")]
	public List<AlertFeederHeader>? Headers { get; set; }

	/// <summary>Whether the alert feeder is active and runs on schedule (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("enabled")]
	public bool? Enabled { get; set; }

	/// <summary>The authentication configuration (secrets travel only in this request body).</summary>
	[JsonPropertyName("auth")]
	public AlertFeederAuth? Auth { get; set; }

	/// <summary>The proxy configuration for the HTTP request.</summary>
	[JsonPropertyName("proxyConfig")]
	public ClientProxyConfig? ProxyConfig { get; set; }

	/// <summary>The maximum time to wait for the external system to respond (the server default is 10 seconds).</summary>
	[JsonPropertyName("requestTimeout")]
	public Interval? RequestTimeout { get; set; }

	/// <summary>The maximum response payload size in bytes (the server default is 10485760); it must be greater than 0.</summary>
	[JsonPropertyName("responseMaxSize")]
	public long? ResponseMaxSize { get; set; }
}
