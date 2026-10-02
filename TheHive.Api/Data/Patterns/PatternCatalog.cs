using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Patterns;

/// <summary>A catalog that groups attack techniques (the spec's <c>OutputCatalogOfPattern</c>). Note the spec's <c>createdBy</c> and <c>createdAt</c> have no leading underscore.</summary>
public sealed class PatternCatalog
{
	/// <summary>The internal identifier (for example <c>~81920</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>CatalogOfPattern</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the catalog.</summary>
	[JsonPropertyName("createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the catalog was created.</summary>
	[JsonPropertyName("createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the catalog was last updated, if it has been updated.</summary>
	[JsonPropertyName("updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The catalog name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The catalog description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The variant identifier (for example a date or matrix name), if any.</summary>
	[JsonPropertyName("variant")]
	public string? Variant { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
