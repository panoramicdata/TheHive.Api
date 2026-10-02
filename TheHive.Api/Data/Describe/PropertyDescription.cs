using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Describe;

/// <summary>
/// One property of an entity model (the spec's <c>PropertyDescription</c>, a <c>oneOf</c> discriminated by <c>type</c>, flattened
/// into one class: only enumeration properties carry <see cref="Values"/> and <see cref="Labels"/>).
/// </summary>
public sealed class PropertyDescription
{
	/// <summary>The property name, as used in <c>filter</c> and <c>sort</c> steps, for example <c>severity</c> or <c>customFields.threat-type</c>.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>How many values the property holds.</summary>
	[JsonPropertyName("cardinality")]
	public PropertyCardinality Cardinality { get; set; }

	/// <summary>Whether the property can be used in aggregations (grouping, counting).</summary>
	[JsonPropertyName("aggregable")]
	public bool Aggregable { get; set; }

	/// <summary>How the property is indexed, which decides how it can be filtered and sorted.</summary>
	[JsonPropertyName("indexType")]
	public PropertyIndexType IndexType { get; set; }

	/// <summary>
	/// The data type: <c>boolean</c>, <c>date</c>, <c>enumeration</c>, <c>float</c>, <c>integer</c>, <c>string</c>, <c>url</c> or
	/// <c>user</c>. Kept as a string because it is the discriminator and newer servers may add types.
	/// </summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>For an enumeration, the allowed values in the format <c>filter</c> steps expect (integers or strings); otherwise empty.</summary>
	[JsonPropertyName("values")]
	public List<JsonElement> Values { get; set; } = [];

	/// <summary>For an enumeration, a human-readable label for each entry of <see cref="Values"/>, in the same order; otherwise empty.</summary>
	[JsonPropertyName("labels")]
	public List<string> Labels { get; set; } = [];
}
