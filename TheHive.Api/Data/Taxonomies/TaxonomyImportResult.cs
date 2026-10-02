using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>
/// The result of a taxonomy ZIP import. The server answers 201 with the spec's <c>Ok</c> (only <see cref="Imported"/>) when every
/// taxonomy was imported, or 207 with <c>OkWithErrors</c> (also <see cref="Errors"/>) when some failed; both are success statuses,
/// so a partial import does not throw: check <see cref="Errors"/>. One type models both, with <see cref="Errors"/> empty for the 201 form.
/// </summary>
public sealed class TaxonomyImportResult
{
	/// <summary>The taxonomies imported successfully.</summary>
	[JsonPropertyName("imported")]
	public List<TaxonomyImported> Imported { get; set; } = [];

	/// <summary>The files that failed to import; empty when everything succeeded.</summary>
	[JsonPropertyName("errors")]
	public List<TaxonomyImportError> Errors { get; set; } = [];
}
