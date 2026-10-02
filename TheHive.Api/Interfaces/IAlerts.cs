using Refit;
using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Observables;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on TheHive alerts.</summary>
public interface IAlerts
{
	/// <summary>Creates an alert (JSON form; see <see cref="AlertCreateRequest"/> for what the multipart form adds).</summary>
	/// <param name="request">The alert to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created alert.</returns>
	[Post("api/v1/alert")]
	Task<Alert> CreateAsync([Body] AlertCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets an alert.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The alert.</returns>
	[Get("api/v1/alert/{alertId}")]
	Task<Alert> GetAsync(string alertId, CancellationToken cancellationToken = default);

	/// <summary>Updates the fields set on <paramref name="request"/>; other fields keep their values.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/alert/{alertId}")]
	Task UpdateAsync(string alertId, [Body] AlertUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes an alert. This cannot be undone; consider closing it instead.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/alert/{alertId}")]
	Task DeleteAsync(string alertId, CancellationToken cancellationToken = default);

	/// <summary>Applies the same field updates to several alerts; fields not set keep their values.</summary>
	/// <param name="request">The alert IDs and the fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/alert/_bulk")]
	Task BulkUpdateAsync([Body] AlertBulkUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes several alerts. This cannot be undone.</summary>
	/// <param name="request">The IDs of the alerts to delete.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/alert/delete/_bulk")]
	Task BulkDeleteAsync([Body] AlertBulkDeleteRequest request, CancellationToken cancellationToken = default);

	/// <summary>Creates a case from an alert; the alert's observables, procedures, attachments, comments and custom fields are copied and the alert is linked to the new case.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The overrides for the values the case inherits from the alert; pass <c>new CaseFromAlertRequest()</c> (sent as <c>{}</c>) to inherit everything. A <see langword="null"/> body would be sent as the JSON literal <c>null</c>, so it is not accepted.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created case.</returns>
	[Post("api/v1/alert/{alertId}/case")]
	Task<Case> CreateCaseAsync(string alertId, [Body] CaseFromAlertRequest request, CancellationToken cancellationToken = default);

	/// <summary>Merges an alert into an existing case: its data is copied, its description is appended to the case description, and the alert is linked to the case and set to the <c>Imported</c> status.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated case.</returns>
	[Post("api/v1/alert/{alertId}/merge/{caseId}")]
	Task<Case> MergeIntoCaseAsync(string alertId, string caseId, CancellationToken cancellationToken = default);

	/// <summary>Imports an alert's observables and procedures into an existing case, without appending the alert description; the alert is linked to the case and set to the <c>Imported</c> status.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated case.</returns>
	[Post("api/v1/alert/{alertId}/import/{caseId}")]
	Task<Case> ImportIntoCaseAsync(string alertId, string caseId, CancellationToken cancellationToken = default);

	/// <summary>Merges several alerts into an existing case (50 at most by default).</summary>
	/// <param name="request">The case and the alerts to merge into it.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated case.</returns>
	[Post("api/v1/alert/merge/_bulk")]
	Task<Case> BulkMergeIntoCaseAsync([Body] AlertBulkMergeRequest request, CancellationToken cancellationToken = default);

	/// <summary>Follows an alert so TheHive resumes automatic updates from MISP when the event changes.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/alert/{alertId}/follow")]
	Task FollowAsync(string alertId, CancellationToken cancellationToken = default);

	/// <summary>Stops following an alert, so changes in its MISP event no longer update it.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/alert/{alertId}/unfollow")]
	Task UnfollowAsync(string alertId, CancellationToken cancellationToken = default);

	/// <summary>Lists the observables an alert shares with another alert or case (at most 100 by default).</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="alertOrCaseId">The ID of the other alert or case, preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shared observables.</returns>
	[Get("api/v1/alert/{alertId}/similar/{alertOrCaseId}/observables")]
	Task<List<Observable>> GetSimilarObservablesAsync(string alertId, string alertOrCaseId, CancellationToken cancellationToken = default);

	/// <summary>Uploads one or more files to an alert as attachments. The spec requires at least one file; an empty list gets a 400 from the server.</summary>
	/// <remarks>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>), so a file is never stored twice; the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the files, so raise it for large uploads.</remarks>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "report.pdf", "application/pdf")</c>; leave the part name unset.</param>
	/// <param name="canRename">Whether the server may rename a file whose name already exists; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	[Multipart]
	[Post("api/v1/alert/{alertId}/attachments")]
	Task<AttachmentUploadResult> AddAttachmentsAsync(
		string alertId,
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		[AliasAs("canRename")] bool? canRename = null,
		CancellationToken cancellationToken = default);

	/// <summary>Removes an attachment from an alert.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/alert/{alertId}/attachment/{attachmentId}")]
	Task DeleteAttachmentAsync(string alertId, string attachmentId, CancellationToken cancellationToken = default);
}
