using Refit;
using TheHive.Api.Data.AlertStatuses;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on configurable alert statuses (the spec tag <c>AlertStatus</c>). Writes require <c>managePlatform</c> and a Gold or Platinum
/// licence; send the <c>X-Organisation: admin</c> header (<see cref="TheHiveClientOptions.Organisation"/>) to target the admin organization.
/// To list statuses use the query API with <c>listAlertStatus</c>.
/// </summary>
public interface IAlertStatuses
{
	/// <summary>Creates a custom alert status.</summary>
	/// <param name="request">The status to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created status.</returns>
	[Post("api/v1/alertStatus")]
	Task<AlertStatus> CreateAsync([Body] AlertStatusCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes an alert status; predefined statuses and statuses still assigned to alerts cannot be deleted (hide them instead).</summary>
	/// <param name="id">The status ID preceded by <c>~</c>, or the status value.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/alertStatus/{id}")]
	Task DeleteAsync(string id, CancellationToken cancellationToken);

	/// <summary>Updates the order, description, colour or visibility of an alert status.</summary>
	/// <param name="id">The status ID preceded by <c>~</c>, or the status value.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/alertStatus/{id}")]
	Task UpdateAsync(string id, [Body] AlertStatusUpdateRequest request, CancellationToken cancellationToken);
}
