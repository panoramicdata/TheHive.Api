using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Tags;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>A taxonomy of tags (the spec's <c>OutputTaxonomy</c>).</summary>
public sealed class Taxonomy : TheHiveEntity
{
	/// <summary>The namespace identifier, which can be used instead of <see cref="TheHive.Api.Data.Common.TheHiveAuditedEntity.Id"/>.</summary>
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
