using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>The license status of the instance, as reported in the platform status (the spec's <c>OutputLicenseStatus</c>).</summary>
public sealed class LicenseStatus : LicenseBase
{
	/// <summary>The unique identifier of the license (for example <c>lic-a1b2c3d4</c>).</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The identifier of the TheHive instance the license is bound to.</summary>
	[JsonPropertyName("instance")]
	[JsonPropertyOrder(200)]
	public string Instance { get; set; } = string.Empty;

	/// <summary>Whether the license is currently valid.</summary>
	[JsonPropertyName("isValid")]
	[JsonPropertyOrder(810)]
	public bool IsValid { get; set; }

	/// <summary>Why the license is not valid; absent when it is valid.</summary>
	[JsonPropertyName("error")]
	[JsonPropertyOrder(820)]
	public string? Error { get; set; }

	/// <summary>The usage and limit of each licensed quota, keyed by capability name.</summary>
	[JsonPropertyName("quotas")]
	[JsonPropertyOrder(830)]
	public Dictionary<string, LicenseQuota> Quotas { get; set; } = [];
}
