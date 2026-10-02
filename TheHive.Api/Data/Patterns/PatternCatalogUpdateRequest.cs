using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Patterns;

/// <summary>The body of an update-catalog request (the spec's <c>InputUpdateCatalogOfPattern</c>). Only set properties are sent; set an <see cref="Optional{T}"/> property to <see langword="null"/> to delete that field.</summary>
public sealed class PatternCatalogUpdateRequest
{
	/// <summary>The new catalog name (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The new description; set to <see langword="null"/> to delete it.</summary>
	[JsonPropertyName("description")]
	public Optional<string?> Description { get; set; }

	/// <summary>The new variant identifier; set to <see langword="null"/> to delete it.</summary>
	[JsonPropertyName("variant")]
	public Optional<string?> Variant { get; set; }
}
