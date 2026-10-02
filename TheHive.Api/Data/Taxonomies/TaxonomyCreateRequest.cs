using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>The body of a create-taxonomy request, in the MISP taxonomy format (the spec's <c>InputTaxonomy</c>).</summary>
public sealed class TaxonomyCreateRequest
{
	/// <summary>The namespace (1 to 128 characters); it must be unique in TheHive.</summary>
	[JsonPropertyName("namespace")]
	public required string Namespace { get; set; }

	/// <summary>A description of what the taxonomy covers (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The version number.</summary>
	[JsonPropertyName("version")]
	public required int Version { get; set; }

	/// <summary>Whether only one predicate can be applied at a time; applying a second one removes the first.</summary>
	[JsonPropertyName("exclusive")]
	public bool? Exclusive { get; set; }

	/// <summary>The predicates (tag labels within the namespace).</summary>
	[JsonPropertyName("predicates")]
	public List<TaxonomyPredicate>? Predicates { get; set; }

	/// <summary>The value sets for predicates that have third-level entries; omit when none have.</summary>
	[JsonPropertyName("values")]
	public List<TaxonomyValue>? Values { get; set; }
}
