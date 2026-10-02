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
	/// <param name="rootId">The case ID preceded by <c>~</c> (the case number is not accepted) to scope the trail to one case; omit it, or pass <c>any</c>, for the most recent entries across all visible object types.</param>
	/// <param name="count">The maximum number of entries to return; the server default is 10, used when this is <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The audit entries, most recent first.</returns>
	[Get("api/v1/flow")]
	Task<List<AuditStreamEntry>> GetFlowAsync([Query] string? rootId = null, [Query] int? count = null, CancellationToken cancellationToken = default);
}
