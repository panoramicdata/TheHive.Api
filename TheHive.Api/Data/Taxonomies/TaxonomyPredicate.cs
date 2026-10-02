using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>A predicate of a taxonomy, a tag label of the form <c>namespace:predicate</c> (the spec's <c>InputPredicate</c>).</summary>
public sealed class TaxonomyPredicate
{
	/// <summary>The identifier used in the tag label (1 to 512 characters).</summary>
	[JsonPropertyName("value")]
	public required string Value { get; set; }

	/// <summary>The full display label, shown instead of the identifier where supported.</summary>
	[JsonPropertyName("expanded")]
	public string? Expanded { get; set; }

	/// <summary>Whether only one entry of this predicate can be selected at a time.</summary>
	[JsonPropertyName("exclusive")]
	public bool? Exclusive { get; set; }

	/// <summary>A description of what the predicate represents.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour (six digits preceded by <c>#</c>) of the predicate tag.</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }
}
