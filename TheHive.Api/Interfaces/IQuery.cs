using Refit;
using System.Text.Json;
using TheHive.Api.Data.Query;

namespace TheHive.Api.Interfaces;

/// <summary>
/// The Query API (listing, searching, filtering, sorting and paging any entity) and query exports. Build requests with
/// <see cref="Querying.QueryBuilder"/> and run them with the typed helpers in <see cref="Querying.QueryExtensions"/>.
/// </summary>
public interface IQuery
{
	/// <summary>Runs a query and returns the raw result.</summary>
	/// <param name="request">The query.</param>
	/// <param name="name">
	/// An optional label for the query, sent as the <c>name</c> query-string parameter. Not in the spec; TheHive 5.8.0 accepts it
	/// (checked against a live server).
	/// </param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The result as returned: usually a JSON array of entities (one per result, with an <c>extraData</c> object when the
	/// <c>page</c> step asks for one), also a one-item array for <c>getXxx</c> queries on TheHive 5.8.0, or a bare number for a <c>count</c> query.
	/// </returns>
	[Post("api/v1/query")]
	Task<JsonElement> RunAsync([Body] QueryRequest request, [Query] string? name = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// UNCHECKED: runs a query and returns the raw response <b>without checking its status</b>. This is the only kind of member
	/// in this client that does not throw <see cref="TheHiveApiException"/> on an error status. It exists so the <c>X-Total</c>
	/// header (the total number of matching results, sent when the <c>page</c> step's <c>extraData</c> includes <c>total</c>)
	/// can be read; prefer <see cref="Querying.QueryExtensions.RunPageAsync"/>, which reads it and throws on errors.
	/// </summary>
	/// <param name="request">The query.</param>
	/// <param name="name">An optional label for the query, as for <see cref="RunAsync"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The raw response; the caller must dispose it and must check <see cref="HttpResponseMessage.IsSuccessStatusCode"/> before
	/// reading the body. On success the body is the JSON result; on an error status it is a TheHive error object
	/// (<c>{"type":...,"message":...}</c>), not the result type.
	/// </returns>
	[Post("api/v1/query")]
	Task<HttpResponseMessage> RunUncheckedAsync(
		[Body] QueryRequest request,
		[Query] string? name = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Runs a query and downloads the results as a file (an unstable route: breaking changes can happen in future releases).
	/// Prefer <see cref="Querying.QueryExtensions.ExportAsync"/>, which writes both parameters from a builder and options.
	/// </summary>
	/// <param name="query">
	/// The query operations as JSON text: the array of steps only, for example <c>[{"_name":"listCase"}]</c> (TheHive 5.8.0 rejects a
	/// whole <c>{"query":[...]}</c> body with 400); URL-encoded as one value.
	/// </param>
	/// <param name="options">The export options as JSON text: <see cref="ExportOptions"/> serialized with <see cref="TheHiveJson.Options"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The file (<c>text/csv</c>, <c>application/json</c> or <c>text/plain</c>), with its name in
	/// <c>Headers.ContentDisposition.FileName</c>; dispose it. Pass a <see cref="CancellationToken"/> when reading it: the
	/// per-attempt timeout does not cover reading the body.
	/// </returns>
	[Get("api/v1/export")]
	Task<HttpContent> ExportAsync([Query] string query, [Query] string options, CancellationToken cancellationToken = default);

	/// <summary>Lists the fields available for CSV export, by model (an unstable route).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The exportable fields of each model.</returns>
	[Get("api/v1/export/_fields")]
	Task<ExportFieldsMapping> GetExportFieldsAsync(CancellationToken cancellationToken = default);
}
