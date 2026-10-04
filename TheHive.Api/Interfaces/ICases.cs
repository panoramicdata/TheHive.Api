using System.ComponentModel;
using Refit;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Timeline;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on TheHive cases.</summary>
public interface ICases
{
	/// <summary>Creates a case.</summary>
	/// <param name="request">The case to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created case.</returns>
	[Post("api/v1/case")]
	Task<Case> CreateAsync([Body] CaseCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Gets a case.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The case.</returns>
	[Get("api/v1/case/{idOrName}")]
	Task<Case> GetAsync(string idOrName, CancellationToken cancellationToken);

	/// <summary>Updates the fields set on <paramref name="request"/>; other fields keep their values.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/{idOrName}")]
	Task UpdateAsync(string idOrName, [Body] CaseUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Permanently deletes a case. This cannot be undone.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{idOrName}")]
	Task DeleteAsync(string idOrName, CancellationToken cancellationToken);

	/// <summary>Merges two or more cases into a new case; the source cases are deleted.</summary>
	/// <param name="ids">The comma-separated case IDs (each preceded by <c>~</c>) or case numbers, for example <c>~1,~2</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new case created by the merge.</returns>
	[Post("api/v1/case/_merge/{ids}")]
	Task<Case> MergeAsync(string ids, CancellationToken cancellationToken);

	/// <summary>Applies the same field updates to several cases; fields not set keep their values.</summary>
	/// <param name="request">The case IDs and the fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/_bulk")]
	Task BulkUpdateAsync([Body] CaseBulkUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Sets the access mode of several cases, which must all have the same initial access (Platinum licence).</summary>
	/// <param name="request">The case IDs and the new access.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/_bulk/access")]
	Task BulkSetAccessAsync([Body] CaseBulkAccessRequest request, CancellationToken cancellationToken);

	/// <summary>Applies a case template to existing cases.</summary>
	/// <param name="request">The case IDs, the template and which of its elements to apply.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/_bulk/caseTemplate")]
	Task BulkApplyTemplateAsync([Body] CaseBulkApplyTemplateRequest request, CancellationToken cancellationToken);

	/// <summary>Sets the access mode of a case (Platinum licence).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The new access.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/access")]
	Task SetAccessAsync(string caseId, [Body] CaseAccessRequest request, CancellationToken cancellationToken);

	/// <summary>Transfers a case to another organization.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The new owner and the profile, if any, the current owner keeps.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/owner")]
	Task ChangeOwnerAsync(string caseId, [Body] CaseOwnerChangeRequest request, CancellationToken cancellationToken);

	/// <summary>Unlinks an alert from a case; the alert returns to a New-stage status and its data stays in the case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{caseId}/alert/{alertId}")]
	Task RemoveAlertAsync(string caseId, string alertId, CancellationToken cancellationToken);

	/// <summary>Deduplicates the observables of a case, merging those with the same type and value.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>How many observables were left, updated and deleted.</returns>
	[Post("api/v1/case/{caseId}/observable/_merge")]
	Task<ObservableDeduplicationResult> DeduplicateObservablesAsync(string caseId, CancellationToken cancellationToken);

	/// <summary>Lists the observables a case shares with another case or alert (at most 100 by default).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="alertOrCaseId">The ID of the other alert or case, preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shared observables.</returns>
	[Get("api/v1/case/{caseId}/similar/{alertOrCaseId}/observables")]
	Task<List<Observable>> GetSimilarObservablesAsync(string caseId, string alertOrCaseId, CancellationToken cancellationToken);

	/// <summary>Gets the activity timeline of a case (Gold or Platinum licence).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The timeline.</returns>
	[Get("api/v1/case/{caseId}/timeline")]
	Task<CaseTimeline> GetTimelineAsync(string caseId, CancellationToken cancellationToken);

	/// <summary>Adds a directed link from a case to another case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The link type and the case to link to.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/link/case/add")]
	Task AddCaseLinkAsync(string caseId, [Body] CaseLinkRequest request, CancellationToken cancellationToken);

	/// <summary>Removes a link to another case; pass the type and case used when adding it.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The link type and the linked case.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/link/case/remove")]
	Task RemoveCaseLinkAsync(string caseId, [Body] CaseLinkRequest request, CancellationToken cancellationToken);

	/// <summary>Adds a link from a case to an external URL.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The link type and URL.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/link/external/add")]
	Task AddExternalLinkAsync(string caseId, [Body] ExternalLinkRequest request, CancellationToken cancellationToken);

	/// <summary>Removes a link to an external URL; pass the type and URL used when adding it.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The link type and URL.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/case/{caseId}/link/external/remove")]
	Task RemoveExternalLinkAsync(string caseId, [Body] ExternalLinkRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the link type names in use across cases.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The link type names.</returns>
	[Get("api/v1/case/link/types")]
	Task<List<string>> GetLinkTypesAsync(CancellationToken cancellationToken);

	/// <summary>Removes a custom field value from a case; the custom field definition is kept.</summary>
	/// <param name="cfId">The custom field value ID preceded by <c>~</c> (the <c>_id</c> in <see cref="Case.CustomFields"/>).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/customField/{cfId}")]
	Task DeleteCustomFieldAsync(string cfId, CancellationToken cancellationToken);

	/// <summary>Uploads one or more files to a case as attachments. The spec requires at least one file; an empty list gets a 400 from the server.</summary>
	/// <remarks>
	/// <para>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>), so a file is never stored twice; the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the files, so raise it for large uploads.</para>
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw multipart transport <see cref="AddAttachmentsMultipartAsync"/>; a class implementing <see cref="ICases"/> only has to provide that method.</para>
	/// </remarks>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "report.pdf", "application/pdf")</c>; leave the part name unset.</param>
	/// <param name="options">The <c>canRename</c> form field (<see cref="AttachmentUploadOptions.CanRename"/>: whether the server may rename a file whose name already exists); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	Task<AttachmentUploadResult> AddAttachmentsAsync(
		string caseId,
		IEnumerable<MultipartItem> attachments,
		AttachmentUploadOptions options,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(options);
		return AddAttachmentsMultipartAsync(caseId, attachments, options.CanRename, cancellationToken);
	}

	/// <summary>
	/// The raw multipart transport used by <see cref="AddAttachmentsAsync"/>, with a <see langword="null"/> <paramref name="canRename"/> left out. Call
	/// <see cref="AddAttachmentsAsync"/> instead; this method exists because Refit cannot turn a property of an object into a multipart form field.
	/// </summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "report.pdf", "application/pdf")</c>; leave the part name unset.</param>
	/// <param name="canRename">Whether the server may rename a file whose name already exists; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Multipart]
	[Post("api/v1/case/{caseId}/attachments")]
	Task<AttachmentUploadResult> AddAttachmentsMultipartAsync(
		string caseId,
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		[AliasAs("canRename")] bool? canRename,
		CancellationToken cancellationToken);

	/// <summary>Updates the properties of a case attachment (Platinum licence).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/{caseId}/attachment/{attachmentId}")]
	Task UpdateAttachmentAsync(string caseId, string attachmentId, [Body] AttachmentUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Removes an attachment from a case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{caseId}/attachment/{attachmentId}")]
	Task DeleteAttachmentAsync(string caseId, string attachmentId, CancellationToken cancellationToken);

	/// <summary>Exports a case as a password-protected THAR archive (Gold or Platinum licence).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="password">The password that encrypts the archive; needed again by <see cref="ImportAsync"/>. Sent in the query string.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The archive. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the suggested file name is in
	/// <c>Headers.ContentDisposition.FileName</c>. The caller owns the content and must dispose it.
	/// </returns>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// </remarks>
	[Get("api/v1/case/{caseId}/export")]
	Task<HttpContent> ExportAsync(string caseId, [Query] string password, CancellationToken cancellationToken);

	/// <summary>Creates a case from a THAR archive made by <see cref="ExportAsync"/> (Gold or Platinum licence).</summary>
	/// <param name="request">The archive password and sharing settings, sent as the JSON part named <c>_json</c>.</param>
	/// <param name="file">The archive, sent as the part named <c>file</c>, for example <c>new StreamPart(stream, "7.thar", "application/octet-stream")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The imported case with its restored observables and procedures.</returns>
	[Multipart]
	[Post("api/v1/case/import")]
	Task<CaseImportResult> ImportAsync(
		[AliasAs("_json")] CaseImportRequest request,
		[AliasAs("file")] MultipartItem file,
		CancellationToken cancellationToken);
}
