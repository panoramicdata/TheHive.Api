using Refit;
using TheHive.Api.Data.Shares;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on case shares, and on sharing tasks and observables with linked organizations. The spec files the case, task and
/// observable share operations under both <c>Share</c> and the entity tag; they are all here. Writes require <c>manageShare</c>.
/// The unshare operations send a JSON body with <c>DELETE</c>, as the spec defines.
/// </summary>
public interface IShares
{
	/// <summary>Lists the shares of a case, with each organization's profile and task and observable rules.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shares.</returns>
	[Get("api/v1/case/{caseId}/shares")]
	Task<List<Share>> ListByCaseAsync(string caseId, CancellationToken cancellationToken);

	/// <summary>Shares a case with linked organizations. This only creates shares; it never updates or removes existing ones (see <see cref="SetCaseSharesAsync"/>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The sharing configurations to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shares created.</returns>
	[Post("api/v1/case/{caseId}/shares")]
	Task<List<Share>> ShareCaseAsync(string caseId, [Body] ShareCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Sets the shares of a case to exactly the given list: new ones are created, existing ones updated and ones not listed removed.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The complete list of sharing configurations.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The resulting shares.</returns>
	[Put("api/v1/case/{caseId}/shares")]
	Task<List<Share>> SetCaseSharesAsync(string caseId, [Body] ShareCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Removes the sharing of a case from the given organizations.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The organizations to remove.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{caseId}/shares")]
	Task UnshareCaseAsync(string caseId, [Body] ShareRemoveRequest request, CancellationToken cancellationToken);

	/// <summary>Removes a single share by its ID, revoking the organization's access to the case and its tasks and observables.</summary>
	/// <param name="shareId">The share ID preceded by <c>~</c>; see <see cref="Share.Id"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/share/{shareId}")]
	Task DeleteAsync(string shareId, CancellationToken cancellationToken);

	/// <summary>Updates the permission profile of a share.</summary>
	/// <param name="shareId">The share ID preceded by <c>~</c>; see <see cref="Share.Id"/>.</param>
	/// <param name="request">The new profile.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/share/{shareId}")]
	Task UpdateAsync(string shareId, [Body] ShareUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Removes several shares by ID.</summary>
	/// <param name="request">The IDs of the shares to remove.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/shares")]
	Task DeleteManyAsync([Body] ShareBulkDeleteRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the shares of a task.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shares.</returns>
	[Get("api/v1/task/{taskId}/shares")]
	Task<List<Share>> ListByTaskAsync(string taskId, CancellationToken cancellationToken);

	/// <summary>Shares a task with linked organizations within an already shared case; useful when the task sharing rule is <c>manual</c>.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="request">The organizations to share with.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/task/{taskId}/shares")]
	Task ShareTaskAsync(string taskId, [Body] ShareOrganisationsRequest request, CancellationToken cancellationToken);

	/// <summary>Removes the sharing of a task from the given organizations.</summary>
	/// <param name="taskId">The task ID preceded by <c>~</c>.</param>
	/// <param name="request">The organizations to remove.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/task/{taskId}/shares")]
	Task UnshareTaskAsync(string taskId, [Body] ShareRemoveRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the shares of an observable.</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The shares.</returns>
	[Get("api/v1/observable/{observableId}/shares")]
	Task<List<Share>> ListByObservableAsync(string observableId, CancellationToken cancellationToken);

	/// <summary>Shares an observable with linked organizations within an already shared case.</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="request">The organizations to share with.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/observable/{observableId}/shares")]
	Task ShareObservableAsync(string observableId, [Body] ShareOrganisationsRequest request, CancellationToken cancellationToken);

	/// <summary>Removes the sharing of an observable from the given organizations.</summary>
	/// <param name="observableId">The observable ID preceded by <c>~</c>.</param>
	/// <param name="request">The organizations to remove.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/observable/{observableId}/shares")]
	Task UnshareObservableAsync(string observableId, [Body] ShareRemoveRequest request, CancellationToken cancellationToken);
}
