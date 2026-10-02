using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Licenses;

namespace TheHive.Api.Data.Status;

/// <summary>The status of the TheHive instance (the spec's <c>OutputStatus</c>).</summary>
public sealed class PlatformStatus
{
	/// <summary>The version of the TheHive instance (for example <c>5.8.0</c>).</summary>
	[JsonPropertyName("version")]
	public string Version { get; set; } = string.Empty;

	/// <summary>The abbreviated commit hash TheHive was built from.</summary>
	[JsonPropertyName("gitDescription")]
	public string GitDescription { get; set; } = string.Empty;

	/// <summary>The status of each connector configured on the instance, keyed by connector name; each value is an untyped object (for example <c>{"enabled":true,"status":"OK"}</c>).</summary>
	[JsonPropertyName("connectors")]
	public Dictionary<string, JsonElement> Connectors { get; set; } = [];

	/// <summary>The non-sensitive authentication and platform configuration (an untyped object: <c>authType</c>, <c>multifactor</c>, <c>capabilities</c>, <c>pollingDuration</c> and more).</summary>
	[JsonPropertyName("config")]
	public JsonElement? Config { get; set; }

	/// <summary>The license status of the instance.</summary>
	[JsonPropertyName("license")]
	public LicenseStatus License { get; set; } = new();

	/// <summary>The Pekko cluster state; filled only when the status was requested with <c>verbose</c> (an untyped object).</summary>
	[JsonPropertyName("cluster")]
	public JsonElement? Cluster { get; set; }

	/// <summary>The database schema version of each module (untyped objects such as <c>{"name":"thehiveCore","currentVersion":42,"expectedVersion":42}</c>); filled only when requested with <c>verbose</c>.</summary>
	[JsonPropertyName("schemaStatus")]
	public List<JsonElement> SchemaStatus { get; set; } = [];

	/// <summary>The feature flags enabled on the instance.</summary>
	[JsonPropertyName("features")]
	public List<string> Features { get; set; } = [];
}
