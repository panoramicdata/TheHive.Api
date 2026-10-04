using System.ComponentModel;
using Refit;
using TheHive.Api.Data.Branding;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Branding operations: the browser tab title, login page logo, navigation bar logo and favicon (the spec tag <c>Branding</c>). Reading is open to
/// every caller; writing requires <c>managePlatform</c> and a Platinum licence, and the <c>X-Organisation: admin</c> header
/// (<see cref="TheHiveClientOptions.Organisation"/>) targets the admin organization.
/// </summary>
public interface IBranding
{
	/// <summary>Gets the title and logo URLs currently configured.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The branding settings.</returns>
	[Get("api/v1/branding")]
	Task<BrandingSettings> GetAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the browser tab title and the login logo, navigation bar logo and favicon (a multipart <c>POST api/v1/branding</c>). Every property of
	/// <paramref name="request"/> is optional; a <see langword="null"/> one is left out of the request and keeps its current value. Uploads are never retried.
	/// </summary>
	/// <param name="request">The title and images to set (the <c>title</c>, <c>loginLogo</c>, <c>menuLogo</c> and <c>favicon</c> parts).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The resulting branding settings.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// This is the method to call. It is implemented on the interface and sends the request through the raw multipart transport
	/// <see cref="SetMultipartAsync"/>. A class implementing <see cref="IBranding"/> only has to provide that method; a mocking library may
	/// intercept this method itself (NSubstitute does), so set this method up directly on a mock.
	/// </remarks>
	Task<BrandingSettings> SetAsync(BrandingUpdateRequest request, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(request);
		return SetMultipartAsync(request.Title, request.LoginLogo, request.MenuLogo, request.Favicon, cancellationToken);
	}

	/// <summary>
	/// The raw multipart transport used by <see cref="SetAsync(BrandingUpdateRequest, CancellationToken)"/>: one form field or part per argument, a
	/// <see langword="null"/> one left out. Call <see cref="SetAsync(BrandingUpdateRequest, CancellationToken)"/> instead; this method exists because Refit
	/// cannot turn one object into several multipart parts.
	/// </summary>
	/// <param name="title">The <c>title</c> form field.</param>
	/// <param name="loginLogo">The <c>loginLogo</c> part.</param>
	/// <param name="menuLogo">The <c>menuLogo</c> part.</param>
	/// <param name="favicon">The <c>favicon</c> part.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The resulting branding settings.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Multipart]
	[Post("api/v1/branding")]
	Task<BrandingSettings> SetMultipartAsync(
		[AliasAs("title")] string? title,
		[AliasAs("loginLogo")] MultipartItem? loginLogo,
		[AliasAs("menuLogo")] MultipartItem? menuLogo,
		[AliasAs("favicon")] MultipartItem? favicon,
		CancellationToken cancellationToken);

	/// <summary>Removes a branding asset, reverting to the default TheHive branding for it.</summary>
	/// <param name="kind">The asset: one of the <see cref="BrandingAssetKinds"/> constants (<c>loginLogo</c>, <c>menuLogo</c>, <c>favicon</c>).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/branding/assets/{kind}")]
	Task DeleteAssetAsync(string kind, CancellationToken cancellationToken = default);

	/// <summary>Downloads a branding image.</summary>
	/// <param name="kind">The asset: one of the <see cref="BrandingAssetKinds"/> constants (<c>loginLogo</c>, <c>menuLogo</c>, <c>favicon</c>).</param>
	/// <param name="options">The <c>If-None-Match</c> header (<see cref="ConditionalDownloadOptions.IfNoneMatch"/>, the <c>ETag</c> of a previous response); pass <c>new()</c> to download unconditionally.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is in <c>Headers</c> of the response, which is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// The server sends <c>Content-Security-Policy</c> and <c>Referrer-Policy</c> headers that callers who serve the image on should keep.
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw transport <see cref="GetAssetWithHeadersAsync"/>; a class implementing <see cref="IBranding"/> only has to provide that method.</para>
	/// </remarks>
	Task<HttpContent> GetAssetAsync(
		string kind,
		ConditionalDownloadOptions options,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		return GetAssetWithHeadersAsync(kind, options.IfNoneMatch, cancellationToken);
	}

	/// <summary>
	/// The raw transport used by <see cref="GetAssetAsync"/>, with a <see langword="null"/> <paramref name="ifNoneMatch"/> left out. Call
	/// <see cref="GetAssetAsync"/> instead; this method exists because Refit cannot turn a property of an object into a request header.
	/// </summary>
	/// <param name="kind">The asset: one of the <see cref="BrandingAssetKinds"/> constants (<c>loginLogo</c>, <c>menuLogo</c>, <c>favicon</c>).</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>. When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is in <c>Headers</c> of the response, which is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Get("api/v1/branding/assets/{kind}")]
	Task<HttpContent> GetAssetWithHeadersAsync(
		string kind,
		[Header("If-None-Match")] string? ifNoneMatch,
		CancellationToken cancellationToken);
}
