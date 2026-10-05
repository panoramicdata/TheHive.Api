using Refit;
using TheHive.Api.Data.Taxonomies;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on taxonomies of tags. To list taxonomies use the query API with <c>listTaxonomy</c>. Taxonomy writes require
/// <c>manageTaxonomy</c> and the <c>X-Organisation: admin</c> header; activation applies to every organization.
/// </summary>
public interface ITaxonomies
{
	/// <summary>Creates a taxonomy from the MISP taxonomy format. It is inactive until <see cref="ActivateAsync"/> is called.</summary>
	/// <param name="request">The taxonomy to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created taxonomy.</returns>
	[Post("api/v1/taxonomy")]
	Task<Taxonomy> CreateAsync([Body] TaxonomyCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Imports taxonomies from a ZIP archive containing at least one MISP-format <c>machinetag.json</c>. Taxonomies with an existing namespace are updated, new ones created inactive.</summary>
	/// <param name="file">The ZIP archive, sent as the multipart part named <c>file</c>. Build it with a file name and, ideally, a content type, for example <c>new StreamPart(stream, "taxonomies.zip", "application/zip")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The imported taxonomies and any per-file errors. A partial import (HTTP 207) is returned, not thrown: check
	/// <see cref="TaxonomyImportResult.Errors"/>.
	/// </returns>
	/// <remarks>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>); the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the file, so raise it for large archives.</remarks>
	[Multipart]
	[Post("api/v1/taxonomy/import-zip")]
	Task<TaxonomyImportResult> ImportZipAsync([AliasAs("file")] MultipartItem file, CancellationToken cancellationToken);

	/// <summary>Gets a taxonomy.</summary>
	/// <param name="taxonomyId">The taxonomy ID preceded by <c>~</c>, or its namespace name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The taxonomy.</returns>
	[Get("api/v1/taxonomy/{taxonomyId}")]
	Task<Taxonomy> GetAsync(string taxonomyId, CancellationToken cancellationToken);

	/// <summary>Permanently deletes a taxonomy and its tags, removing them from every case, case template, alert and observable that uses them.</summary>
	/// <param name="taxonomyId">The taxonomy ID preceded by <c>~</c>, or its namespace name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/taxonomy/{taxonomyId}")]
	Task DeleteAsync(string taxonomyId, CancellationToken cancellationToken);

	/// <summary>Activates a taxonomy in every organization, making its tags available.</summary>
	/// <param name="taxonomyId">The taxonomy ID preceded by <c>~</c>, or its namespace name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/taxonomy/{taxonomyId}/activate")]
	Task ActivateAsync(string taxonomyId, CancellationToken cancellationToken);

	/// <summary>Deactivates a taxonomy in every organization, removing its tags from the catalog; the data is kept and tags already in use stay attached.</summary>
	/// <param name="taxonomyId">The taxonomy ID preceded by <c>~</c>, or its namespace name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/taxonomy/{taxonomyId}/deactivate")]
	Task DeactivateAsync(string taxonomyId, CancellationToken cancellationToken);
}
