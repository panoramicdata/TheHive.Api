using Refit;
using TheHive.Api.Data.Cortex;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on the Cortex connector: analyzers, analyzer jobs, analyzer report templates and responder actions. A Cortex server
/// must be connected to TheHive. Cortex server credentials are never part of these operations.
/// </summary>
public interface ICortex
{
	/// <summary>Runs a Cortex responder on a case, alert, observable, task or task log. Cortex processes it asynchronously: the returned action is <see cref="CortexActionStatus.Waiting"/>. Requires <c>manageAction</c>.</summary>
	/// <param name="request">The responder and target entity.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new action.</returns>
	[Post("api/v1/connector/cortex/action")]
	Task<CortexAction> CreateActionAsync([Body] CortexActionInput request, CancellationToken cancellationToken = default);

	/// <summary>Runs several responders in one call, each on its own entity and processed independently. Requires <c>manageAction</c>.</summary>
	/// <param name="requests">The responders to run; pass a non-null list.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new actions.</returns>
	[Post("api/v1/connector/cortex/actions")]
	Task<List<CortexAction>> CreateActionsAsync([Body] IEnumerable<CortexActionInput> requests, CancellationToken cancellationToken = default);

	/// <summary>Lists the analyzers available to the organization across every connected Cortex server.</summary>
	/// <param name="range"><c>all</c> for every analyzer, or Cortex's <c>start-end</c> range such as <c>0-25</c>; omit for Cortex's default <c>0-10</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The analyzers.</returns>
	[Get("api/v1/connector/cortex/analyzer")]
	Task<List<CortexAnalyzer>> ListAnalyzersAsync([Query] string? range = null, CancellationToken cancellationToken = default);

	/// <summary>Gets an analyzer by its ID, searching every Cortex server available to the organization.</summary>
	/// <param name="analyzerId">The analyzer ID.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The analyzer.</returns>
	[Get("api/v1/connector/cortex/analyzer/{analyzerId}")]
	Task<CortexAnalyzer> GetAnalyzerAsync(string analyzerId, CancellationToken cancellationToken = default);

	/// <summary>Creates an analyzer report template. Requires <c>manageAnalyzerTemplate</c> (use the <c>X-Organisation: admin</c> header to target the admin organization).</summary>
	/// <param name="request">The analyzer and template content.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created template.</returns>
	[Post("api/v1/connector/cortex/analyzer/template")]
	Task<AnalyzerTemplate> CreateAnalyzerTemplateAsync([Body] AnalyzerTemplateCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Imports analyzer templates in bulk from a ZIP archive, such as the official report-templates archive. Requires <c>manageAnalyzerTemplate</c>.</summary>
	/// <param name="templates">The ZIP archive, sent as the multipart part named <c>templates</c>. Build it with a file name and, ideally, a content type, for example <c>new StreamPart(stream, "report-templates.zip", "application/zip")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>Whether each analyzer's template was imported, keyed by analyzer name.</returns>
	/// <remarks>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>); the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the file.</remarks>
	[Multipart]
	[Post("api/v1/connector/cortex/analyzer/template/_import")]
	Task<Dictionary<string, bool>> ImportAnalyzerTemplatesAsync([AliasAs("templates")] MultipartItem templates, CancellationToken cancellationToken = default);

	/// <summary>Deletes an analyzer template; reports then use the default rendering. Requires <c>manageAnalyzerTemplate</c>.</summary>
	/// <param name="analyzerTemplateId">The template ID preceded by <c>~</c>, or the name of the analyzer it is linked to.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/connector/cortex/analyzer/template/{analyzerTemplateId}")]
	Task DeleteAnalyzerTemplateAsync(string analyzerTemplateId, CancellationToken cancellationToken = default);

	/// <summary>Updates the HTML content of an analyzer template. Requires <c>manageAnalyzerTemplate</c>.</summary>
	/// <param name="analyzerTemplateId">The template ID preceded by <c>~</c>, or the name of the analyzer it is linked to.</param>
	/// <param name="request">The new content.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated template.</returns>
	[Patch("api/v1/connector/cortex/analyzer/template/{analyzerTemplateId}")]
	Task<AnalyzerTemplate> UpdateAnalyzerTemplateAsync(string analyzerTemplateId, [Body] AnalyzerTemplateUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets the HTML content of an analyzer template.</summary>
	/// <param name="analyzerId">The template ID preceded by <c>~</c>, or the name of the analyzer it is linked to.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The raw <c>text/plain</c> body.</returns>
	[Get("api/v1/connector/cortex/analyzer/template/content/{analyzerId}")]
	Task<string> GetAnalyzerTemplateContentAsync(string analyzerId, CancellationToken cancellationToken = default);

	/// <summary>Lists the analyzers that support an observable data type.</summary>
	/// <param name="dataType">The observable data type, for example <c>ip</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The analyzers.</returns>
	[Get("api/v1/connector/cortex/analyzer/type/{dataType}")]
	Task<List<CortexAnalyzer>> ListAnalyzersByTypeAsync(string dataType, CancellationToken cancellationToken = default);

	/// <summary>Runs an analyzer on an observable, creating a job that Cortex processes asynchronously. Requires <c>manageAnalyse</c>.</summary>
	/// <param name="request">The analyzer, Cortex server and observable.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new job.</returns>
	[Post("api/v1/connector/cortex/job")]
	Task<CortexJob> CreateJobAsync([Body] CortexJobCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets an analyzer job, including its status and, once complete, its report.</summary>
	/// <param name="jobId">The job ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The job.</returns>
	[Get("api/v1/connector/cortex/job/{jobId}")]
	Task<CortexJob> GetJobAsync(string jobId, CancellationToken cancellationToken = default);

	/// <summary>Lists the responders available for one entity, filtered by its TLP and PAP levels.</summary>
	/// <param name="entityType"><c>case</c>, <c>case_artifact</c> (observable), <c>case_task</c>, <c>case_task_log</c> or <c>alert</c>.</param>
	/// <param name="entityId">The entity ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The responders.</returns>
	[Get("api/v1/connector/cortex/responder/{entityType}/{entityId}")]
	Task<List<CortexEntityResponder>> ListRespondersAsync(string entityType, string entityId, CancellationToken cancellationToken = default);

	/// <summary>Lists the responders that can run on an entity type, whatever the TLP and PAP of a given entity. Requires <c>manageAction</c>.</summary>
	/// <param name="entityType"><c>case</c>, <c>case_artifact</c> (observable), <c>case_task</c>, <c>case_task_log</c> or <c>alert</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The responders.</returns>
	[Get("api/v1/connector/cortex/responders/{entityType}")]
	Task<List<CortexResponder>> ListRespondersForEntityTypeAsync(string entityType, CancellationToken cancellationToken = default);

	/// <summary>Lists the responder executions (actions) linked to a case and its tasks, observables and task logs, or to an alert and its observables. Runs from before this endpoint existed are not returned.</summary>
	/// <param name="scope"><c>case</c> or <c>alert</c>.</param>
	/// <param name="rootId">The case ID preceded by <c>~</c> or the case number when <paramref name="scope"/> is <c>case</c>, or the alert ID preceded by <c>~</c>.</param>
	/// <param name="filter">A filter as JSON text, in the syntax of the <c>filter</c> operation of the query API without its <c>_name</c>, for example <c>{"_eq":{"_field":"status","_value":"Success"}}</c>. The spec declares this parameter as JSON-encoded content; it is URL-encoded as one query value.</param>
	/// <param name="sort">Sort criteria as JSON text, in the syntax of the <c>_fields</c> value of the query API's <c>sort</c> operation, for example <c>[{"field":"responderId","direction":"asc"}]</c>.</param>
	/// <param name="pageFrom">The 0-based index of the first result (default 0).</param>
	/// <param name="pageTo">The exclusive index of the last result (default <paramref name="pageFrom"/> + 30; the range cannot exceed 300).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The actions; the response carries no total, use <see cref="CountActionsAsync"/>.</returns>
	[Get("api/v1/responder-execution/{scope}/{rootId}")]
	Task<List<CortexAction>> ListActionsAsync(
		string scope,
		string rootId,
		[Query] string? filter = null,
		[Query] string? sort = null,
		[Query] int? pageFrom = null,
		[Query] int? pageTo = null,
		CancellationToken cancellationToken = default);

	/// <summary>Counts the responder executions (actions) linked to a case or an alert, matching an optional filter.</summary>
	/// <param name="scope"><c>case</c> or <c>alert</c>.</param>
	/// <param name="rootId">The case ID preceded by <c>~</c> or the case number when <paramref name="scope"/> is <c>case</c>, or the alert ID preceded by <c>~</c>.</param>
	/// <param name="filter">A filter as JSON text, as for <see cref="ListActionsAsync"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The number of matching actions.</returns>
	[Get("api/v1/responder-execution/{scope}/{rootId}/count")]
	Task<int> CountActionsAsync(string scope, string rootId, [Query] string? filter = null, CancellationToken cancellationToken = default);
}
