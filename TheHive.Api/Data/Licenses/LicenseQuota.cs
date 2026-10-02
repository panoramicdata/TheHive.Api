using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>The usage and limit of one licensed quota (the spec's <c>OutputLicenseQuota</c>).</summary>
public sealed class LicenseQuota
{
	/// <summary>The current usage; absent when usage is not tracked for the capability.</summary>
	[JsonPropertyName("current")]
	public int? Current { get; set; }

	/// <summary>The maximum allowed value; <c>-1</c> means unlimited.</summary>
	[JsonPropertyName("quota")]
	public int Quota { get; set; }
}
