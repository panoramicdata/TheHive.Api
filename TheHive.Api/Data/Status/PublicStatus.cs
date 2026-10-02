using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Status;

/// <summary>Platform information that needs no authentication (the spec's <c>OutputPublicStatus</c>).</summary>
public sealed class PublicStatus
{
	/// <summary>Whether single sign-on is turned on.</summary>
	[JsonPropertyName("sso")]
	public bool Sso { get; set; }

	/// <summary>The SSO providers available on the sign-in page.</summary>
	[JsonPropertyName("ssoProviders")]
	public List<SsoProvider> SsoProviders { get; set; } = [];

	/// <summary>The version of the TheHive instance (for example <c>5.8.0</c>).</summary>
	[JsonPropertyName("version")]
	public string Version { get; set; } = string.Empty;

	/// <summary>The status of the built-in reference data imports.</summary>
	[JsonPropertyName("imports")]
	public ImportsStatus Imports { get; set; } = new();
}
