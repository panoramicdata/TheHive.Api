using Refit;

namespace TheHive.Api.Data.Branding;

/// <summary>
/// The multipart form of <c>IBranding.SetAsync</c>: the browser tab title and the login logo, navigation bar logo and favicon. Every property is
/// optional; a <see langword="null"/> one is left out of the request and keeps its current value.
/// </summary>
/// <remarks>
/// The images are Refit <see cref="MultipartItem"/>s, as in every other upload of this client: pass a <see cref="StreamPart"/>, <see cref="ByteArrayPart"/>
/// or <see cref="FileInfoPart"/> with a file name and content type, and leave the part's own name unset (the part name comes from the property).
/// </remarks>
public sealed class BrandingUpdateRequest
{
	/// <summary>The title shown in the browser tab (1 to 128 characters), sent as the <c>title</c> form field.</summary>
	public string? Title { get; set; }

	/// <summary>The login page logo, PNG or JPEG, recommended 200 x 200 pixels, sent as the <c>loginLogo</c> part.</summary>
	public MultipartItem? LoginLogo { get; set; }

	/// <summary>The navigation bar logo, PNG or JPEG, recommended 84 x 84 pixels, sent as the <c>menuLogo</c> part.</summary>
	public MultipartItem? MenuLogo { get; set; }

	/// <summary>The browser tab icon, PNG or JPEG, recommended 42 x 42 pixels, sent as the <c>favicon</c> part.</summary>
	public MultipartItem? Favicon { get; set; }
}
