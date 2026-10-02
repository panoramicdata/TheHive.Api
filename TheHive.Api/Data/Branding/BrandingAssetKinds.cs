namespace TheHive.Api.Data.Branding;

/// <summary>The branding asset kinds accepted by <c>IBranding.GetAssetAsync</c> and <c>IBranding.DeleteAssetAsync</c> (the spec types the path parameter as a string and lists these three values).</summary>
public static class BrandingAssetKinds
{
	/// <summary>The logo displayed on the login page.</summary>
	public const string LoginLogo = "loginLogo";

	/// <summary>The logo displayed in the navigation bar.</summary>
	public const string MenuLogo = "menuLogo";

	/// <summary>The icon displayed in the browser tab.</summary>
	public const string Favicon = "favicon";
}
