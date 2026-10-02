using Refit;
using TheHive.Api.Data.Dashboards;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on dashboards. Writes require <c>manageDashboard</c>; to list dashboards use the query API with <c>listDashboard</c>.</summary>
public interface IDashboards
{
	/// <summary>Creates a dashboard.</summary>
	/// <param name="request">The dashboard to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created dashboard.</returns>
	[Post("api/v1/dashboard")]
	Task<Dashboard> CreateAsync([Body] DashboardCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a dashboard; this cannot be undone.</summary>
	/// <param name="dashboardId">The dashboard ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/dashboard/{dashboardId}")]
	Task DeleteAsync(string dashboardId, CancellationToken cancellationToken = default);

	/// <summary>Gets a dashboard: one shared with your organization, or a private one you own.</summary>
	/// <param name="dashboardId">The dashboard ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The dashboard.</returns>
	[Get("api/v1/dashboard/{dashboardId}")]
	Task<Dashboard> GetAsync(string dashboardId, CancellationToken cancellationToken = default);

	/// <summary>Updates a dashboard; only the properties set on the request change. Setting the status to <see cref="DashboardStatus.Deleted"/> deletes it.</summary>
	/// <param name="dashboardId">The dashboard ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/dashboard/{dashboardId}")]
	Task UpdateAsync(string dashboardId, [Body] DashboardUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Transfers a dashboard to another user. Requires <c>manageDashboard</c> and <c>manageUser</c>, and the dashboard must be shared: private dashboards cannot be transferred.</summary>
	/// <param name="dashboardId">The dashboard ID preceded by <c>~</c>.</param>
	/// <param name="request">The new owner.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/dashboard/{dashboardId}/owner")]
	Task ChangeOwnerAsync(string dashboardId, [Body] DashboardOwnerChangeRequest request, CancellationToken cancellationToken = default);
}
