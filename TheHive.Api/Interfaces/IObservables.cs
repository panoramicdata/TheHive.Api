using Refit;
using TheHive.Api.Data.Observables;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on observables.</summary>
public interface IObservables
{
	/// <summary>Adds one or more observables to a case (requires <c>manageObservable</c>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The observable to create; one observable is created per value in <c>Data</c>.
	/// Only the JSON form is modelled: for a file observable, reference an attachment already stored in TheHive. That needs the storage ID (<c>Attachment.StorageId</c>, not the <c>~…</c> <c>_id</c>) of a file from the organization attachment upload (<c>POST /api/v1/attachment</c>, see <see cref="IOrganisations.UploadAttachmentsAsync"/>).</param>
	/// <param name="dataType">The observable type, used by the server only when <c>request.DataType</c> is missing; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created observables.</returns>
	[Post("api/v1/case/{caseId}/observable")]
	Task<List<Observable>> CreateInCaseAsync(
		string caseId,
		[Body] ObservableInput request,
		[Query] string? dataType = null,
		CancellationToken cancellationToken = default);

	/// <summary>Adds one or more observables to an alert (requires <c>manageObservable</c>).</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The observable to create; one observable is created per value in <c>Data</c>.
	/// Only the JSON form is modelled: for a file observable, reference an attachment already stored in TheHive. That needs the storage ID (<c>Attachment.StorageId</c>, not the <c>~…</c> <c>_id</c>) of a file from the organization attachment upload (<c>POST /api/v1/attachment</c>, see <see cref="IOrganisations.UploadAttachmentsAsync"/>).</param>
	/// <param name="dataType">The observable type, used by the server only when <c>request.DataType</c> is missing; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created observables.</returns>
	[Post("api/v1/alert/{alertId}/observable")]
	Task<List<Observable>> CreateInAlertAsync(
		string alertId,
		[Body] ObservableInput request,
		[Query] string? dataType = null,
		CancellationToken cancellationToken = default);

	/// <summary>Gets an observable with its type, value, tags and analysis reports.</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The observable.</returns>
	[Get("api/v1/observable/{observableId}")]
	Task<Observable> GetAsync(string observableId, CancellationToken cancellationToken = default);

	/// <summary>Updates an observable; only the set properties of the request change (requires <c>manageObservable</c>).</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/observable/{observableId}")]
	Task UpdateAsync(string observableId, [Body] ObservableUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Applies the same update to several observables (requires <c>manageObservable</c>).</summary>
	/// <param name="request">The observable IDs and the properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/observable/_bulk")]
	Task BulkUpdateAsync([Body] ObservableBulkUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes an observable; to keep it but exclude it from correlation, set <c>IgnoreSimilarity</c> instead (requires <c>manageObservable</c>).</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/observable/{observableId}")]
	Task DeleteAsync(string observableId, CancellationToken cancellationToken = default);

	/// <summary>Downloads the file attached to a file-type observable.</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="asZip">When <see langword="true"/>, the file is wrapped in a password-protected ZIP archive (default password <c>malware</c>) named <c>{name}.zip</c>, content type <c>application/zip</c>; omitted when <see langword="null"/>. Verified against TheHive 5.8: <see langword="false"/> and <see langword="null"/> both return the file itself.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the suggested file name is in
	/// <c>Headers.ContentDisposition.FileName</c>. The caller owns the content and must dispose it.
	/// </returns>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// </remarks>
	[Get("api/v1/observable/{observableId}/attachment/{attachmentId}/download")]
	Task<HttpContent> DownloadAttachmentAsync(
		string observableId,
		string attachmentId,
		[Query] bool? asZip = null,
		CancellationToken cancellationToken = default);
}
