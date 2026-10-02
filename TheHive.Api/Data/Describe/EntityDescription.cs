using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Describe;

/// <summary>The metadata of an entity model (the spec's <c>EntityDescription</c>): the fields queries can filter, sort and aggregate on.</summary>
public sealed class EntityDescription
{
	/// <summary>The model name, for example <c>case</c>.</summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = string.Empty;

	/// <summary>Reserved for future use; currently always empty.</summary>
	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;

	/// <summary>The primary <c>list</c> operation for this model in <c>POST /api/v1/query</c>, for example <c>listCase</c>; empty if the model is not queryable.</summary>
	[JsonPropertyName("initialQuery")]
	public string InitialQuery { get; set; } = string.Empty;

	/// <summary>The model's properties, including metadata fields such as <c>_createdBy</c> and <c>_createdAt</c>.</summary>
	[JsonPropertyName("attributes")]
	public List<PropertyDescription> Attributes { get; set; } = [];
}
