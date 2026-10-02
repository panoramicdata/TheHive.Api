using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>The license status of the instance, as reported in the platform status (the spec's <c>OutputLicenseStatus</c>).</summary>
public sealed class LicenseStatus
{
	/// <summary>The unique identifier of the license (for example <c>lic-a1b2c3d4</c>).</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The name of the customer the license is issued to.</summary>
	[JsonPropertyName("customer")]
	public string Customer { get; set; } = string.Empty;

	/// <summary>The identifier of the TheHive instance the license is bound to.</summary>
	[JsonPropertyName("instance")]
	public string Instance { get; set; } = string.Empty;

	/// <summary>The license plan: <c>No</c>, <c>Community</c>, <c>Gold</c> or <c>Platinum</c>. Kept as the string the server sent.</summary>
	[JsonPropertyName("plan")]
	public string Plan { get; set; } = string.Empty;

	/// <summary>The license kind: <c>Regular</c>, <c>Trial</c> or <c>Development</c>. Kept as the string the server sent.</summary>
	[JsonPropertyName("kind")]
	public string Kind { get; set; } = string.Empty;

	/// <summary>When the license became valid.</summary>
	[JsonPropertyName("validFrom")]
	public DateTimeOffset ValidFrom { get; set; }

	/// <summary>When the license expires.</summary>
	[JsonPropertyName("expiresAt")]
	public DateTimeOffset ExpiresAt { get; set; }

	/// <summary>The capabilities unlocked by the license plan.</summary>
	[JsonPropertyName("capabilities")]
	public List<string> Capabilities { get; set; } = [];

	/// <summary>Whether the license is currently valid.</summary>
	[JsonPropertyName("isValid")]
	public bool IsValid { get; set; }

	/// <summary>Why the license is not valid; absent when it is valid.</summary>
	[JsonPropertyName("error")]
	public string? Error { get; set; }

	/// <summary>The usage and limit of each licensed quota, keyed by capability name.</summary>
	[JsonPropertyName("quotas")]
	public Dictionary<string, LicenseQuota> Quotas { get; set; } = [];
}
