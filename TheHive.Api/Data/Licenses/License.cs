using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>A license stored in TheHive (the spec's <c>OutputLicense</c>). It never carries the license key.</summary>
public sealed class License : LicenseBase
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>When the license was added to TheHive.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who added the license.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>License</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The license identifier from the license file (for example <c>LIC-GOLD-2025-001</c>); also accepted wherever a license ID is taken.</summary>
	[JsonPropertyName("id")]
	public string LicenseId { get; set; } = string.Empty;

	/// <summary>Whether this is the currently active license.</summary>
	[JsonPropertyName("current")]
	[JsonPropertyOrder(700)]
	public bool Current { get; set; }

	/// <summary>The resource quotas defined by the license, mapping a capability name to its limit; <c>-1</c> means unlimited.</summary>
	[JsonPropertyName("quotas")]
	[JsonPropertyOrder(900)]
	public Dictionary<string, int> Quotas { get; set; } = [];
}
