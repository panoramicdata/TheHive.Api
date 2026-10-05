namespace TheHive.Api.Data.Common;

/// <summary>
/// The optional request header of a conditional download (an attachment, avatar or branding image). Pass an empty instance (<c>new()</c>) to
/// download unconditionally.
/// </summary>
public sealed class ConditionalDownloadOptions
{
	/// <summary>
	/// The <c>ETag</c> of a previous response, sent as the <c>If-None-Match</c> header; left out when <see langword="null"/>. When it still matches,
	/// the server answers 304 and the download method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.
	/// </summary>
	public string? IfNoneMatch { get; set; }
}