using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>A custom field value as returned by TheHive.</summary>
public sealed class CustomFieldValue
{
	/// <summary>The internal identifier of the custom field value.</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The custom field name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The custom field type (for example <c>string</c>, <c>integer</c>, <c>boolean</c>, <c>date</c>, <c>float</c> or <c>url</c>).</summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The value; its JSON type depends on <see cref="Type"/>.</summary>
	[JsonPropertyName("value")]
	public JsonElement Value { get; set; }

	/// <summary>The display order.</summary>
	[JsonPropertyName("order")]
	public int Order { get; set; }
}
