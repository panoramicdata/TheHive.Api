using Refit;
using TheHive.Api.Data.Patterns;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on attack techniques (patterns, such as MITRE ATT&amp;CK) and the catalogs that group them (the spec tag <c>Att&amp;ck</c>).
/// Writes require <c>managePattern</c>; send the <c>X-Organisation: admin</c> header (<see cref="TheHiveClientOptions.Organisation"/>) to target the admin
/// organization. To list techniques and catalogs use the query API with <c>listPattern</c> and <c>listCatalog</c>.
/// </summary>
public interface IPatterns
{
	/// <summary>Creates a technique catalog.</summary>
	/// <param name="request">The catalog to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created catalog.</returns>
	[Post("api/v1/catalog")]
	Task<PatternCatalog> CreateCatalogAsync([Body] PatternCatalogCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a technique catalog.</summary>
	/// <param name="catalogId">The catalog ID preceded by <c>~</c>, or the catalog name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/catalog/{catalogId}")]
	Task DeleteCatalogAsync(string catalogId, CancellationToken cancellationToken);

	/// <summary>Updates a technique catalog.</summary>
	/// <param name="catalogId">The catalog ID preceded by <c>~</c>, or the catalog name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/catalog/{catalogId}")]
	Task UpdateCatalogAsync(string catalogId, [Body] PatternCatalogUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a technique.</summary>
	/// <param name="patternId">The technique ID preceded by <c>~</c>, or its MITRE identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/pattern/{patternId}")]
	Task DeleteAsync(string patternId, CancellationToken cancellationToken);

	/// <summary>Gets a technique.</summary>
	/// <param name="patternId">The technique ID preceded by <c>~</c>, or its MITRE identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The technique.</returns>
	[Get("api/v1/pattern/{patternId}")]
	Task<Pattern> GetAsync(string patternId, CancellationToken cancellationToken);

	/// <summary>Lists the techniques linked to a case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The techniques.</returns>
	[Get("api/v1/pattern/case/{caseId}")]
	Task<List<Pattern>> ListForCaseAsync(string caseId, CancellationToken cancellationToken);

	/// <summary>
	/// Imports MITRE ATT&amp;CK techniques into a catalog from the URL of a catalog JSON file (<see cref="PatternImportRequest.Url"/>). The server fetches the URL.
	/// A partial import (HTTP 207) is returned, not thrown: check <see cref="PatternImportResult.Errors"/>.
	/// </summary>
	/// <param name="request">The catalog, variant and source URL.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The import result.</returns>
	[Post("api/v1/pattern/import/attack")]
	Task<PatternImportResult> ImportAsync([Body] PatternImportRequest request, CancellationToken cancellationToken);

	/// <summary>
	/// Imports MITRE ATT&amp;CK techniques into a catalog from an uploaded JSON file (the multipart form of the same operation as
	/// <see cref="ImportAsync"/>). A partial import (HTTP 207) is returned, not thrown. Uploads are never retried.
	/// </summary>
	/// <param name="request">The catalog and variant, sent as the <c>_json</c> part (leave <see cref="PatternImportRequest.Url"/> unset).</param>
	/// <param name="file">The catalog JSON file (a <c>StreamPart</c>, <c>ByteArrayPart</c> or <c>FileInfoPart</c> with a file name), sent as the <c>file</c> part.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The import result.</returns>
	[Multipart]
	[Post("api/v1/pattern/import/attack")]
	Task<PatternImportResult> ImportFileAsync(
		[AliasAs("_json")] PatternImportRequest request,
		[AliasAs("file")] MultipartItem file,
		CancellationToken cancellationToken);
}
