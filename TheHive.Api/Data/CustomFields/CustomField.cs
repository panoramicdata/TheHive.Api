using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CustomFields;

/// <summary>A custom field definition (the spec's <c>OutputCustomField</c>).</summary>
public sealed class CustomField
{
	/// <summary>The internal identifier (for example <c>~123456789</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>customField</c>.</summary>
	[JsonPropertyName("_type")]
	public string EntityType { get; set; } = string.Empty;

	/// <summary>The login of the user who created the custom field.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the custom field, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the custom field was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the custom field was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The technical name, used as the key when setting values on cases and alerts.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The user-facing name.</summary>
	[JsonPropertyName("displayName")]
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>The group that organizes related custom fields.</summary>
	[JsonPropertyName("group")]
	public string Group { get; set; } = string.Empty;

	/// <summary>The description shown to users.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The data type of the values.</summary>
	[JsonPropertyName("type")]
	public CustomFieldType Type { get; set; }

	/// <summary>The predefined selectable values: all strings, all integers or all floats, depending on <see cref="Type"/>; empty when users may enter any value.</summary>
	[JsonPropertyName("options")]
	public List<JsonElement> Options { get; set; } = [];

	/// <summary>Whether users must fill the field in before a case or alert can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool Mandatory { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
