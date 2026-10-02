using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CustomFields;

/// <summary>The body of an update-custom-field request (the spec's <c>InputUpdateCustomField</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class CustomFieldUpdateRequest
{
	/// <summary>The new user-facing name (1 to 64 characters).</summary>
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	/// <summary>The new group (1 to 64 characters).</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The new description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new data type.</summary>
	[JsonPropertyName("type")]
	public CustomFieldType? Type { get; set; }

	/// <summary>Whether users must fill the field in before a case or alert can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool? Mandatory { get; set; }

	/// <summary>The predefined selectable values; they replace the current ones. Available for the string, integer and float types.</summary>
	[JsonPropertyName("options")]
	public List<object>? Options { get; set; }
}
