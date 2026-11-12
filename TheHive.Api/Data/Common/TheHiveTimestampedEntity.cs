using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>A stored object whose audit fields are named without an underscore (<c>createdAt</c>, <c>updatedAt</c>), alongside <c>_id</c> and <c>_type</c>.</summary>
public abstract class TheHiveTimestampedEntity
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The object type name.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the object.</summary>
	[JsonPropertyName("createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the object was created.</summary>
	[JsonPropertyName("createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the object was last updated, if it has been updated.</summary>
	[JsonPropertyName("updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }
}
