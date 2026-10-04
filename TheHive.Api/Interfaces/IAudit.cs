using Refit;
using TheHive.Api.Data.AuditTrail;

namespace TheHive.Api.Interfaces;

/// <summary>Audit trail operations (the spec tag <c>Audit</c>).</summary>
public interface IAudit
{
	/// <summary>
	/// Gets the most recent audit trail entries for a case and its linked objects, or the most recent entries across the supported object types
	/// when no case is specified (the data behind the case History tab and the Live Feed). Without a case only actions on cases, alerts, tasks,
	/// observables, organizations, case templates, dashboards, shares, jobs, attachments, actions and comments appear.
	/// </summary>
	/// <param name="query">
	/// The case to scope the trail to (<see cref="AuditFlowQuery.RootId"/>) and the maximum number of entries (<see cref="AuditFlowQuery.Count"/>); pass
	/// <c>new()</c> for the most recent entries across all visible object types.
	/// </param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The audit entries, most recent first.</returns>
	[Get("api/v1/flow")]
	Task<List<AuditStreamEntry>> GetFlowAsync([Query] AuditFlowQuery query, CancellationToken cancellationToken = default);
}
