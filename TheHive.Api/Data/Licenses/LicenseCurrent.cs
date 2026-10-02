using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>
/// The current license status of the instance (the spec's <c>OutputLicenseCurrent</c>, a <c>oneOf</c> of three variants without a discriminator), flattened
/// into one type: a valid license sets <see cref="License"/> only; a license that failed validation sets <see cref="Error"/> and <see cref="Fallback"/>;
/// no active license sets <see cref="Fallback"/> and <see cref="NotFound"/>.
/// </summary>
public sealed class LicenseCurrent
{
	/// <summary>The active license, when it is valid and within its validity period.</summary>
	[JsonPropertyName("license")]
	public License? License { get; set; }

	/// <summary>When the active license failed validation, the license that failed; when none is configured, the placeholder license of an unlicensed instance.</summary>
	[JsonPropertyName("fallback")]
	public License? Fallback { get; set; }

	/// <summary>Why the active license failed validation (for example <c>License has expired</c>), if it did.</summary>
	[JsonPropertyName("error")]
	public string? Error { get; set; }

	/// <summary><see langword="true"/> when no active license is configured; absent otherwise.</summary>
	[JsonPropertyName("notFound")]
	public bool? NotFound { get; set; }
}
