using Refit;
using TheHive.Api.Data.TaskLogs;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on task logs and their attachments.</summary>
public interface ITaskLogs
{
	/// <summary>Creates a log on a task, whatever the task's status (requires <c>manageTask</c>).</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="request">The log to create; the JSON form only, add files with <see cref="AddAttachmentsAsync"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created log.</returns>
	[Post("api/v1/task/{taskId}/log")]
	Task<TaskLog> CreateAsync(string taskId, [Body] TaskLogCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Updates the content or timeline pin of a task log (requires <c>manageTask</c>).</summary>
	/// <param name="logId">The task log ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/log/{logId}")]
	Task UpdateAsync(string logId, [Body] TaskLogUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a task log (requires <c>manageTask</c>).</summary>
	/// <param name="logId">The task log ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/log/{logId}")]
	Task DeleteAsync(string logId, CancellationToken cancellationToken = default);

	/// <summary>Adds files to an existing task log (requires <c>manageTask</c>).</summary>
	/// <param name="logId">The task log ID preceded by <c>~</c>.</param>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "memory.txt", "text/plain")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <remarks>The server answers 204 with no body, so the created attachments are not returned; read them from the log.</remarks>
	[Multipart]
	[Post("api/v1/log/{logId}/attachments")]
	Task AddAttachmentsAsync(
		string logId,
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes an attachment from a task log (requires <c>manageTask</c>).</summary>
	/// <param name="logId">The task log ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/log/{logId}/attachments/{attachmentId}")]
	Task DeleteAttachmentAsync(string logId, string attachmentId, CancellationToken cancellationToken = default);

	/// <summary>Gets the binary content of an attachment linked to an observable (the spec files this operation under the Task Log tag).</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="attachmentId">The attachment ID preceded by <c>~</c>.</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// </remarks>
	[Get("api/v1/observable/{observableId}/attachment/{attachmentId}")]
	Task<HttpContent> GetObservableAttachmentAsync(
		string observableId,
		string attachmentId,
		[Header("If-None-Match")] string? ifNoneMatch = null,
		CancellationToken cancellationToken = default);
}
