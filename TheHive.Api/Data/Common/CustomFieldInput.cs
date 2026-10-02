using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>A custom field value to set (the array form of the spec's <c>customFields</c> input).</summary>
public sealed class CustomFieldInput
{
	/// <summary>The name of an existing custom field.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The value: a string, integer, boolean, epoch-millisecond date, float or URL, depending on the field definition.</summary>
	[JsonPropertyName("value")]
	[JsonIgnore(Condition = JsonIgnoreCondition.Never)]
	public required object? Value { get; set; }

	/// <summary>The display order, if any.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }
}
