using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The connection settings shared by the create, update and test alert-feeder request bodies. Unset properties are omitted.</summary>
public abstract class AlertFeederSettings
{
	/// <summary>A description of the alert feeder (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	[JsonPropertyOrder(2)]
	public required string Description { get; set; }

	/// <summary>The HTTP method used to request data.</summary>
	[JsonPropertyName("method")]
	[JsonPropertyOrder(3)]
	public required AlertFeederMethod Method { get; set; }

	/// <summary>The endpoint URL of the external system.</summary>
	[JsonPropertyName("url")]
	[JsonPropertyOrder(4)]
	public required string Url { get; set; }

	/// <summary>The polling interval; the minimum is 1 minute.</summary>
	[JsonPropertyName("interval")]
	[JsonPropertyOrder(5)]
	public required Interval Interval { get; set; }

	/// <summary>The HTTP request body to send; used when the method is <c>POST</c>. The spec declares a string, but its own examples and curl samples send a JSON object, so this is raw JSON: a string is written as a JSON string and an object as an object, and either form reads (verify against a live server). Build it with <c>JsonSerializer.SerializeToElement</c>; a default (undefined) <see cref="JsonElement"/> cannot be written, so leave the property <see langword="null"/> to omit it.</summary>
	[JsonPropertyName("body")]
	[JsonPropertyOrder(7)]
	public JsonElement? Body { get; set; }

	/// <summary>The HTTP headers to include in the request; on update they replace the current set. Header values can be secrets: they travel only in this request body.</summary>
	[JsonPropertyName("headers")]
	[JsonPropertyOrder(8)]
	public List<AlertFeederHeader>? Headers { get; set; }

	/// <summary>Whether the alert feeder is active and runs on schedule (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("enabled")]
	[JsonPropertyOrder(9)]
	public bool? Enabled { get; set; }

	/// <summary>The authentication configuration (secrets travel only in this request body).</summary>
	[JsonPropertyName("auth")]
	[JsonPropertyOrder(10)]
	public AlertFeederAuth? Auth { get; set; }

	/// <summary>The proxy configuration for the HTTP request.</summary>
	[JsonPropertyName("proxyConfig")]
	[JsonPropertyOrder(11)]
	public ClientProxyConfig? ProxyConfig { get; set; }

	/// <summary>The maximum time to wait for the external system to respond (the server default is 10 seconds).</summary>
	[JsonPropertyName("requestTimeout")]
	[JsonPropertyOrder(12)]
	public Interval? RequestTimeout { get; set; }

	/// <summary>The maximum response payload size in bytes (the server default is 10485760); it must be greater than 0.</summary>
	[JsonPropertyName("responseMaxSize")]
	[JsonPropertyOrder(13)]
	public long? ResponseMaxSize { get; set; }
}
