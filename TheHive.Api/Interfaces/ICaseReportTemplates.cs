using System.ComponentModel;
using Refit;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.CaseReportTemplates;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on case report templates: the layouts (widgets, header, footer) used to generate case reports, and the attachments
/// their <c>Image</c> widgets use. To list templates use the query API with <c>listCaseReportTemplate</c>. Requires a Platinum licence;
/// writes require <c>manageCaseReportTemplate</c>. Generating reports is on <see cref="ICaseReports"/>.
/// </summary>
public interface ICaseReportTemplates
{
	/// <summary>Creates a case report template (requires <c>manageCaseReportTemplate</c>).</summary>
	/// <param name="request">The template to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created template.</returns>
	[Post("api/v1/caseReportTemplate")]
	Task<CaseReportTemplate> CreateAsync([Body] CaseReportTemplateCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the widget types and the field names each widget category supports, to discover valid values for a template definition.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The available options.</returns>
	[Get("api/v1/caseReportTemplate/_info")]
	Task<CaseReportTemplateOptions> GetOptionsAsync(CancellationToken cancellationToken);

	/// <summary>Gets a case report template.</summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The template.</returns>
	[Get("api/v1/caseReportTemplate/{templateId}")]
	Task<CaseReportTemplate> GetAsync(string templateId, CancellationToken cancellationToken);

	/// <summary>Updates a case report template (requires <c>manageCaseReportTemplate</c>); only the properties set on the request change.</summary>
	/// <param name="idOrName">The template ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/caseReportTemplate/{idOrName}")]
	Task UpdateAsync(string idOrName, [Body] CaseReportTemplateUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Permanently deletes a case report template (requires <c>manageCaseReportTemplate</c>).</summary>
	/// <param name="idOrName">The template ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/caseReportTemplate/{idOrName}")]
	Task DeleteAsync(string idOrName, CancellationToken cancellationToken);

	/// <summary>Uploads one or more files to a template as attachments, for use in <c>Image</c> widgets (requires <c>manageCaseReportTemplate</c>). The spec requires at least one file.</summary>
	/// <remarks>
	/// <para>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>), so a file is never stored twice; the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the files, so raise it for large uploads.</para>
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw multipart transport <see cref="AddAttachmentsMultipartAsync"/>; a class implementing <see cref="ICaseReportTemplates"/> only has to provide that method.</para>
	/// </remarks>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "logo.png", "image/png")</c>; leave the part name unset.</param>
	/// <param name="options">The <c>canRename</c> form field (<see cref="AttachmentUploadOptions.CanRename"/>: whether the server may rename a file whose name already exists); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	Task<AttachmentUploadResult> AddAttachmentsAsync(
		string templateId,
		IEnumerable<MultipartItem> attachments,
		AttachmentUploadOptions options,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(options);
		return AddAttachmentsMultipartAsync(templateId, attachments, options.CanRename, cancellationToken);
	}

	/// <summary>
	/// The raw multipart transport used by <see cref="AddAttachmentsAsync"/>, with a <see langword="null"/> <paramref name="canRename"/> left out. Call
	/// <see cref="AddAttachmentsAsync"/> instead; this method exists because Refit cannot turn a property of an object into a multipart form field.
	/// </summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "logo.png", "image/png")</c>; leave the part name unset.</param>
	/// <param name="canRename">Whether the server may rename a file whose name already exists; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Multipart]
	[Post("api/v1/caseReportTemplate/{templateId}/attachment")]
	Task<AttachmentUploadResult> AddAttachmentsMultipartAsync(
		string templateId,
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		[AliasAs("canRename")] bool? canRename,
		CancellationToken cancellationToken);

	/// <summary>Permanently deletes an attachment from a template (requires <c>manageCaseReportTemplate</c>).</summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/caseReportTemplate/{templateId}/attachment/{attachmentId}")]
	Task DeleteAttachmentAsync(string templateId, string attachmentId, CancellationToken cancellationToken);

	/// <summary>Streams the content of a template attachment.</summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
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
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw transport <see cref="GetAttachmentWithHeadersAsync"/>; a class implementing <see cref="ICaseReportTemplates"/> only has to provide that method.</para>
	/// </remarks>
	Task<HttpContent> GetAttachmentAsync(
		string templateId,
		string attachmentId,
		ConditionalDownloadOptions options,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(options);
		return GetAttachmentWithHeadersAsync(templateId, attachmentId, options.IfNoneMatch, cancellationToken);
	}

	/// <summary>
	/// The raw transport used by <see cref="GetAttachmentAsync"/>, with a <see langword="null"/> <paramref name="ifNoneMatch"/> left out. Call
	/// <see cref="GetAttachmentAsync"/> instead; this method exists because Refit cannot turn a property of an object into a request header.
	/// </summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Get("api/v1/caseReportTemplate/{templateId}/attachment/{attachmentId}")]
	Task<HttpContent> GetAttachmentWithHeadersAsync(
		string templateId,
		string attachmentId,
		[Header("If-None-Match")] string? ifNoneMatch,
		CancellationToken cancellationToken);

	/// <summary>Downloads a template attachment, with its name in the <c>Content-Disposition</c> header.</summary>
	/// <param name="templateId">The template ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file; the suggested name is in <c>Headers.ContentDisposition.FileName</c>. The caller owns the content and must dispose it.
	/// </returns>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// </remarks>
	[Get("api/v1/caseReportTemplate/{templateId}/attachment/{attachmentId}/download")]
	Task<HttpContent> DownloadAttachmentAsync(string templateId, string attachmentId, CancellationToken cancellationToken);
}
