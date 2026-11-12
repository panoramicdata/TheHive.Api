using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Patterns;

/// <summary>A catalog that groups attack techniques (the spec's <c>OutputCatalogOfPattern</c>). Note the spec's <c>createdBy</c> and <c>createdAt</c> have no leading underscore.</summary>
public sealed class PatternCatalog : TheHiveTimestampedEntity
{
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
