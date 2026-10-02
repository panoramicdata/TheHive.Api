using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Branding;

/// <summary>The title and logo URLs configured for TheHive (the spec's <c>OutputBranding</c>).</summary>
public sealed class BrandingSettings
{
	/// <summary>The title shown in the browser tab instead of TheHive, if set.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The relative URL of the login page logo (for example <c>api/v1/branding/assets/loginLogo</c>), if set; fetch the image with <c>IBranding.GetAssetAsync</c>.</summary>
	[JsonPropertyName("loginLogo")]
	public string? LoginLogo { get; set; }

	/// <summary>The relative URL of the navigation bar logo, if set.</summary>
	[JsonPropertyName("menuLogo")]
	public string? MenuLogo { get; set; }

	/// <summary>The relative URL of the browser tab favicon, if set.</summary>
	[JsonPropertyName("favicon")]
	public string? Favicon { get; set; }
}
