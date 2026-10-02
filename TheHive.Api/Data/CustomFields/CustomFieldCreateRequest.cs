using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CustomFields;

/// <summary>The body of a create-custom-field request (the spec's <c>InputCustomField</c>). Unset properties are omitted.</summary>
public sealed class CustomFieldCreateRequest
{
	/// <summary>The technical name (1 to 64 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The user-facing name shown in cases and alerts (1 to 64 characters).</summary>
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	/// <summary>The group that organizes related custom fields, shown as a tab in cases and alerts (1 to 64 characters).</summary>
	[JsonPropertyName("group")]
	public required string Group { get; set; }

	/// <summary>The description shown to users when they hover over the field.</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The data type of the values.</summary>
	[JsonPropertyName("type")]
	public required CustomFieldType Type { get; set; }

	/// <summary>Whether users must fill the field in before a case or alert can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool? Mandatory { get; set; }

	/// <summary>The predefined selectable values, for the <see cref="CustomFieldType.String"/>, <see cref="CustomFieldType.Integer"/> and <see cref="CustomFieldType.Float"/> types; users can then only pick from these.</summary>
	[JsonPropertyName("options")]
	public List<object>? Options { get; set; }
}
