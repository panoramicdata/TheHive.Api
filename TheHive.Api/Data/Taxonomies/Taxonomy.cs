using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Tags;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>A taxonomy of tags (the spec's <c>OutputTaxonomy</c>).</summary>
public sealed class Taxonomy
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Taxonomy</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the taxonomy.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the taxonomy, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the taxonomy was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the taxonomy was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The namespace identifier, which can be used instead of <see cref="Id"/>.</summary>
	[JsonPropertyName("namespace")]
	public string Namespace { get; set; } = string.Empty;

	/// <summary>A description of what the taxonomy covers.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The version number.</summary>
	[JsonPropertyName("version")]
	public int Version { get; set; }

	/// <summary>The tags of the taxonomy; empty when the server does not return them.</summary>
	[JsonPropertyName("tags")]
	public List<Tag> Tags { get; set; } = [];

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
