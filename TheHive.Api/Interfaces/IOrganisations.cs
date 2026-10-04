using System.ComponentModel;
using Refit;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Organisations;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on organizations, their sharing links, sharing profiles and organization-level files. Organization writes require
/// <c>manageOrganisation</c>; send the <c>X-Organisation: admin</c> header to target the admin organization. To list organizations use
/// the query API with <c>listOrganisation</c>. The spec files the <c>/api/v1/attachment</c> operations under the Organization tag.
/// </summary>
public interface IOrganisations
{
	/// <summary>Creates an organization. Only the JSON form is modelled; set an avatar afterwards with <see cref="UpdateAsync"/>.</summary>
	/// <param name="request">The organization to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created organization.</returns>
	[Post("api/v1/organisation")]
	Task<Organisation> CreateAsync([Body] OrganisationCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets an organization.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The organization.</returns>
	[Get("api/v1/organisation/{orgId}")]
	Task<Organisation> GetAsync(string orgId, CancellationToken cancellationToken = default);

	/// <summary>Updates an organization; only the properties set on the request change.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/organisation/{orgId}")]
	Task UpdateAsync(string orgId, [Body] OrganisationUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Streams the avatar image of an organization.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="fileHash">The hash of the avatar file, the last segment of an avatar path such as <c>api/v1/organisation/~1048576/avatar/fake-avatar-hash</c>.</param>
	/// <param name="options">The <c>If-None-Match</c> header (<see cref="ConditionalDownloadOptions.IfNoneMatch"/>, the <c>ETag</c> of a previous response); pass <c>new()</c> to download unconditionally.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw transport <see cref="GetAvatarWithHeadersAsync"/>; a class implementing <see cref="IOrganisations"/> only has to provide that method.</para>
	/// </remarks>
	Task<HttpContent> GetAvatarAsync(
		string orgId,
		string fileHash,
		ConditionalDownloadOptions options,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		return GetAvatarWithHeadersAsync(orgId, fileHash, options.IfNoneMatch, cancellationToken);
	}

	/// <summary>
	/// The raw transport used by <see cref="GetAvatarAsync"/>, with a <see langword="null"/> <paramref name="ifNoneMatch"/> left out. Call
	/// <see cref="GetAvatarAsync"/> instead; this method exists because Refit cannot turn a property of an object into a request header.
	/// </summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="fileHash">The hash of the avatar file, the last segment of an avatar path such as <c>api/v1/organisation/~1048576/avatar/fake-avatar-hash</c>.</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Get("api/v1/organisation/{orgId}/avatar/{fileHash}")]
	Task<HttpContent> GetAvatarWithHeadersAsync(
		string orgId,
		string fileHash,
		[Header("If-None-Match")] string? ifNoneMatch,
		CancellationToken cancellationToken);

	/// <summary>Removes the sharing link between two organizations; they can then no longer share cases with each other.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="otherOrgId">The ID preceded by <c>~</c>, or name, of the organization to unlink.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/organisation/{orgId}/link/{otherOrgId}")]
	Task UnlinkAsync(string orgId, string otherOrgId, CancellationToken cancellationToken = default);

	/// <summary>Creates a sharing link between two organizations so they can share cases.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="otherOrgId">The ID preceded by <c>~</c>, or name, of the organization to link with.</param>
	/// <param name="request">The sharing profile for each direction; pass <c>new OrganisationLinkRequest()</c> to use the <c>default</c> profile for both (the spec's body is optional, but a <see langword="null"/> body would be sent as a JSON <c>null</c>).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/organisation/{orgId}/link/{otherOrgId}")]
	Task LinkAsync(string orgId, string otherOrgId, [Body] OrganisationLinkRequest request, CancellationToken cancellationToken = default);

	/// <summary>Lists the organizations linked to an organization, with the sharing profile in each direction.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The links.</returns>
	[Get("api/v1/organisation/{orgId}/links")]
	Task<List<OrganisationLinkDetails>> ListLinksAsync(string orgId, CancellationToken cancellationToken = default);

	/// <summary>Replaces the complete set of sharing links of an organization: existing links are removed and the given ones created.</summary>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The links to apply.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/organisation/{orgId}/links")]
	Task ReplaceLinksAsync(string orgId, [Body] OrganisationBulkLinkRequest request, CancellationToken cancellationToken = default);

	/// <summary>Lists the sharing profiles defined in the platform configuration.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The sharing profiles.</returns>
	[Get("api/v1/sharingProfile")]
	Task<List<SharingProfile>> ListSharingProfilesAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Uploads one or more permanent files to the organization (requires <c>manageKnowledgeBase</c>). This is also how to obtain the
	/// attachment to use for a file observable: pass the returned <see cref="Attachment.StorageId"/> (the wire's <c>id</c>, the hex SHA-256 of the file),
	/// with <see cref="Attachment.Name"/> and <see cref="Attachment.ContentType"/>, in the <c>ObservableAttachmentReference</c> of an <c>ObservableInput</c>,
	/// so upload here first and then create the observable from it. Verified against TheHive 5.8: the <see cref="Attachment.Id"/> (the <c>_id</c>, a <c>~…</c> value,
	/// as in the spec's example) is rejected with 404 <c>Attachment … not found</c>. The observable gets its own attachment entity (a new <c>_id</c>) for the same stored file.
	/// </summary>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "sample.exe", "application/octet-stream")</c>; leave the part name unset.</param>
	/// <param name="options">The <c>canRename</c> form field (<see cref="AttachmentUploadOptions.CanRename"/>: whether the server may rename a file whose name already exists); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <para>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>), so a file is never stored twice; the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the files, so raise it for large uploads.</para>
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw multipart transport <see cref="UploadAttachmentsMultipartAsync"/>; a class implementing <see cref="IOrganisations"/> only has to provide that method.</para>
	/// </remarks>
	Task<AttachmentUploadResult> UploadAttachmentsAsync(
		IEnumerable<MultipartItem> attachments,
		AttachmentUploadOptions options,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		return UploadAttachmentsMultipartAsync(attachments, options.CanRename, cancellationToken);
	}

	/// <summary>
	/// The raw multipart transport used by <see cref="UploadAttachmentsAsync"/>, with a <see langword="null"/> <paramref name="canRename"/> left out. Call
	/// <see cref="UploadAttachmentsAsync"/> instead; this method exists because Refit cannot turn a property of an object into a multipart form field.
	/// </summary>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "sample.exe", "application/octet-stream")</c>; leave the part name unset.</param>
	/// <param name="canRename">Whether the server may rename a file whose name already exists; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Multipart]
	[Post("api/v1/attachment")]
	Task<AttachmentUploadResult> UploadAttachmentsMultipartAsync(
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		[AliasAs("canRename")] bool? canRename,
		CancellationToken cancellationToken);

	/// <summary>Removes a file from the organization (requires <c>manageKnowledgeBase</c>); the stored file may remain if other objects reference it.</summary>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/attachment/{attachmentId}")]
	Task DeleteAttachmentAsync(string attachmentId, CancellationToken cancellationToken = default);

	/// <summary>Streams the content of an organization file.</summary>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="options">The <c>If-None-Match</c> header (<see cref="ConditionalDownloadOptions.IfNoneMatch"/>, the <c>ETag</c> of a previous response); pass <c>new()</c> to download unconditionally.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw transport <see cref="GetAttachmentWithHeadersAsync"/>; a class implementing <see cref="IOrganisations"/> only has to provide that method.</para>
	/// </remarks>
	Task<HttpContent> GetAttachmentAsync(
		string attachmentId,
		ConditionalDownloadOptions options,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(options);
		return GetAttachmentWithHeadersAsync(attachmentId, options.IfNoneMatch, cancellationToken);
	}

	/// <summary>
	/// The raw transport used by <see cref="GetAttachmentAsync"/>, with a <see langword="null"/> <paramref name="ifNoneMatch"/> left out. Call
	/// <see cref="GetAttachmentAsync"/> instead; this method exists because Refit cannot turn a property of an object into a request header.
	/// </summary>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Get("api/v1/attachment/{attachmentId}")]
	Task<HttpContent> GetAttachmentWithHeadersAsync(
		string attachmentId,
		[Header("If-None-Match")] string? ifNoneMatch,
		CancellationToken cancellationToken);

	/// <summary>Downloads an organization file, with its name in the <c>Content-Disposition</c> header.</summary>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the suggested file name is in
	/// <c>Headers.ContentDisposition.FileName</c>. The caller owns the content and must dispose it.
	/// </returns>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// </remarks>
	[Get("api/v1/attachment/{attachmentId}/download")]
	Task<HttpContent> DownloadAttachmentAsync(string attachmentId, CancellationToken cancellationToken = default);
}
