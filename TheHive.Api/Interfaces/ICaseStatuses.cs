using Refit;
using TheHive.Api.Data.CaseStatuses;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on configurable case statuses (the spec tag <c>CaseStatus</c>). Writes require <c>managePlatform</c> and a Gold or Platinum
/// licence; send the <c>X-Organisation: admin</c> header (<see cref="TheHiveClientOptions.Organisation"/>) to target the admin organization.
/// To list statuses use the query API with <c>listCaseStatus</c>.
/// </summary>
public interface ICaseStatuses
{
	/// <summary>Creates a custom case status.</summary>
	/// <param name="request">The status to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created status.</returns>
	[Post("api/v1/caseStatus")]
	Task<CaseStatus> CreateAsync([Body] CaseStatusCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes an case status; predefined statuses and statuses still assigned to cases cannot be deleted (hide them instead).</summary>
	/// <param name="id">The status ID preceded by <c>~</c>, or the status value.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/caseStatus/{id}")]
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Updates the order, description, colour or visibility of an case status.</summary>
	/// <param name="id">The status ID preceded by <c>~</c>, or the status value.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/caseStatus/{id}")]
	Task UpdateAsync(string id, [Body] CaseStatusUpdateRequest request, CancellationToken cancellationToken = default);
}
