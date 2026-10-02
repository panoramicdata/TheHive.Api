using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>A taxonomy imported successfully (the spec's <c>Imported</c>).</summary>
public sealed class TaxonomyImported
{
	/// <summary>The namespace of the imported taxonomy.</summary>
	[JsonPropertyName("namespace")]
	public string Namespace { get; set; } = string.Empty;

	/// <summary>The number of tags imported from the taxonomy.</summary>
	[JsonPropertyName("numberOfTags")]
	public int NumberOfTags { get; set; }

	/// <summary>The import status; always <c>Success</c>.</summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;
}
