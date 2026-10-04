using Refit;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on case tasks.</summary>
public interface ITasks
{
	/// <summary>Creates a task in a case (requires <c>manageTask</c>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The task to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created task.</returns>
	[Post("api/v1/case/{caseId}/task")]
	Task<CaseTask> CreateAsync(string caseId, [Body] CaseTaskCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Gets a task.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The task.</returns>
	[Get("api/v1/task/{taskId}")]
	Task<CaseTask> GetAsync(string taskId, CancellationToken cancellationToken);

	/// <summary>Updates a task; only the set properties of the request change (requires <c>manageTask</c>).</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/task/{taskId}")]
	Task UpdateAsync(string taskId, [Body] CaseTaskUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Applies the same update to several tasks (requires <c>manageTask</c>).</summary>
	/// <param name="request">The task IDs and the properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/task/_bulk")]
	Task BulkUpdateAsync([Body] CaseTaskBulkUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Permanently deletes a task; consider setting its status to <c>Completed</c> or <c>Cancel</c> instead (requires <c>manageTask</c>).</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/task/{taskId}")]
	Task DeleteAsync(string taskId, CancellationToken cancellationToken);

	/// <summary>Tells, for each organization sharing a task, whether action is required on it.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Whether action is required, keyed by organization name.</returns>
	[Get("api/v1/task/{taskId}/actionRequired")]
	Task<Dictionary<string, bool>> GetActionRequiredAsync(string taskId, CancellationToken cancellationToken);

	/// <summary>Marks a task as requiring action from an organization; this does not change the task status.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or the organization name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/task/{taskId}/actionRequired/{orgId}")]
	Task SetActionRequiredAsync(string taskId, string orgId, CancellationToken cancellationToken);

	/// <summary>Clears the action required from an organization on a task; this does not change the task status.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="orgId">The organization ID preceded by <c>~</c>, or the organization name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/task/{taskId}/actionDone/{orgId}")]
	Task SetActionDoneAsync(string taskId, string orgId, CancellationToken cancellationToken);
}
