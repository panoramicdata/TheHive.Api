using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>The entries under one predicate, forming tags of the form <c>namespace:predicate:entry</c> (the spec's <c>InputValue</c>).</summary>
public sealed class TaxonomyValue
{
	/// <summary>The identifier of the predicate these entries belong to (1 to 512 characters).</summary>
	[JsonPropertyName("predicate")]
	public required string Predicate { get; set; }

	/// <summary>The entries.</summary>
	[JsonPropertyName("entry")]
	public List<TaxonomyEntry>? Entry { get; set; }
}
