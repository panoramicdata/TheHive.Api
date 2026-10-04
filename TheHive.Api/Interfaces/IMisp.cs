using System.Text.Json;
using Refit;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Misp;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on the MISP connector: server synchronization status, event synchronization, and case import and export. MISP server credentials are not part of these operations.</summary>
public interface IMisp
{
	/// <summary>Gets the synchronization status of each MISP server visible to the organization: last sync time, last event time, and whether the last sync was partial.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The status as raw JSON; the spec only says object and does not define its fields.</returns>
	[Get("api/v1/connector/misp/status")]
	Task<JsonElement> GetStatusAsync(CancellationToken cancellationToken);

	/// <summary>Triggers a delta synchronization of events from every connected MISP server into alerts. Requires <c>manageOrganisation</c>.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Get("api/v1/connector/misp/_syncAlerts")]
	Task SyncAlertsAsync(CancellationToken cancellationToken);

	/// <summary>Exports a case to a MISP server as an event (only indicators of compromise are included); later IOC updates are synchronized automatically. Requires <c>manageShare</c>, and a server configured for export.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="mispName">The name of the MISP server, as listed by <see cref="GetStatusAsync"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/connector/misp/export/{caseId}/{mispName}")]
	Task ExportCaseAsync(string caseId, string mispName, CancellationToken cancellationToken);

	/// <summary>Creates a case from a MISP event JSON file; the event's observables are transferred to the case. Requires <c>manageCase/create</c>.</summary>
	/// <param name="request">The case settings, sent as the JSON part named <c>_json</c>; pass <c>new MispCaseImportRequest()</c> for none.</param>
	/// <param name="file">The MISP event JSON file, sent as the part named <c>file</c>, for example <c>new StreamPart(stream, "event.json", "application/json")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created case.</returns>
	/// <remarks>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>); the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the file.</remarks>
	[Multipart]
	[Post("api/v1/connector/misp/case/import")]
	Task<Case> ImportCaseAsync(
		[AliasAs("_json")] MispCaseImportRequest request,
		[AliasAs("file")] MultipartItem file,
		CancellationToken cancellationToken);
}
