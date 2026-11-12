using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>The terms shared by a license and its validity status.</summary>
public abstract class LicenseBase
{
	/// <summary>The customer the license was issued to.</summary>
	[JsonPropertyName("customer")]
	[JsonPropertyOrder(100)]
	public string Customer { get; set; } = string.Empty;

	/// <summary>The license plan.</summary>
	[JsonPropertyName("plan")]
	[JsonPropertyOrder(300)]
	public string Plan { get; set; } = string.Empty;

	/// <summary>The kind of license.</summary>
	[JsonPropertyName("kind")]
	[JsonPropertyOrder(400)]
	public string Kind { get; set; } = string.Empty;

	/// <summary>When the license becomes valid.</summary>
	[JsonPropertyName("validFrom")]
	[JsonPropertyOrder(500)]
	public DateTimeOffset ValidFrom { get; set; }

	/// <summary>When the license expires.</summary>
	[JsonPropertyName("expiresAt")]
	[JsonPropertyOrder(600)]
	public DateTimeOffset ExpiresAt { get; set; }

	/// <summary>The capabilities the license grants.</summary>
	[JsonPropertyName("capabilities")]
	[JsonPropertyOrder(800)]
	public List<string> Capabilities { get; set; } = [];
}
