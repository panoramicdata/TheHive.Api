using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Patterns;

/// <summary>The body of a create-catalog request (the spec's <c>InputCatalogOfPattern</c>). Unset properties are omitted.</summary>
public sealed class PatternCatalogCreateRequest
{
	/// <summary>The unique catalog name (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>An optional description (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>An optional variant identifier, such as a date or matrix name (1 to 128 characters).</summary>
	[JsonPropertyName("variant")]
	public string? Variant { get; set; }
}
