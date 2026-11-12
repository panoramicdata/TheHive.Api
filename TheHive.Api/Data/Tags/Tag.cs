using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Tags;

/// <summary>A tag, custom or from a taxonomy (the spec's <c>OutputTag</c>).</summary>
public sealed class Tag : TheHiveEntity
{
	/// <summary>The namespace: <c>_freetags_</c> for custom tags, the taxonomy namespace for taxonomy tags.</summary>
	[JsonPropertyName("namespace")]
	public string Namespace { get; set; } = string.Empty;

	/// <summary>The tag name.</summary>
	[JsonPropertyName("predicate")]
	public string Predicate { get; set; } = string.Empty;

	/// <summary>The value component, used by taxonomy tags with a value; absent for custom tags.</summary>
	[JsonPropertyName("value")]
	public string? Value { get; set; }

	/// <summary>The description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour code (for example <c>#e8560a</c>).</summary>
	[JsonPropertyName("colour")]
	public string Colour { get; set; } = string.Empty;

	/// <summary>Whether the tag is hidden; hidden taxonomy tags cannot be added to cases, alerts and observables but stay on those that already have them.</summary>
	[JsonPropertyName("hidden")]
	public bool Hidden { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
