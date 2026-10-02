using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Comments;

/// <summary>A comment on a case or alert (the spec's <c>OutputComment</c>).</summary>
public sealed class Comment
{
	/// <summary>The internal identifier (for example <c>~843456789</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Comment</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the comment.</summary>
	[JsonPropertyName("createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the comment was created.</summary>
	[JsonPropertyName("createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the comment was last updated, if it has been updated.</summary>
	[JsonPropertyName("updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login of the user who last updated the comment, if it has been updated.</summary>
	[JsonPropertyName("updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>The text of the comment.</summary>
	[JsonPropertyName("message")]
	public string Message { get; set; } = string.Empty;

	/// <summary>Whether the comment has been edited since it was created.</summary>
	[JsonPropertyName("isEdited")]
	public bool IsEdited { get; set; }

	/// <summary>Extra data populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>Whether the comment is visible to external users through TheHive Portal.</summary>
	[JsonPropertyName("external")]
	public bool External { get; set; }
}
