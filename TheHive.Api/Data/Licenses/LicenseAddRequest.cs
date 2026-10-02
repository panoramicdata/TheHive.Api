using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>
/// The body of an add-license request (the spec's <c>InputLicense</c>). The license key is a secret: it is sent in the JSON body only and this class does not print it.
/// </summary>
public sealed class LicenseAddRequest
{
	/// <summary>The raw license key provided by StrangeBee. SECRET: do not log it.</summary>
	[JsonPropertyName("license")]
	public required string License { get; set; }
}
