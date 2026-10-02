using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>An entry (the tag value) under a predicate (the spec's <c>InputEntry</c>).</summary>
public sealed class TaxonomyEntry
{
	/// <summary>The identifier used as the tag value (1 to 512 characters).</summary>
	[JsonPropertyName("value")]
	public required string Value { get; set; }

	/// <summary>The full display label, shown instead of the identifier where supported.</summary>
	[JsonPropertyName("expanded")]
	public string? Expanded { get; set; }

	/// <summary>The hex colour (six digits preceded by <c>#</c>) of the entry tag.</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }

	/// <summary>A description of what the entry represents.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>A numerical score for sorting or weighting.</summary>
	[JsonPropertyName("numerical_value")]
	public int? NumericalValue { get; set; }
}
