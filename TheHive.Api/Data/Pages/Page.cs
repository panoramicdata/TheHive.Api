using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Pages;

/// <summary>A Knowledge Base, case or page-template page (the spec's <c>OutputPage</c>).</summary>
public sealed class Page
{
	/// <summary>The internal identifier (for example <c>~84123456</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Page</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the page.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the page, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the page was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the page was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The content, in TheHive-flavored Markdown.</summary>
	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;

	/// <summary>The display order within the category.</summary>
	[JsonPropertyName("order")]
	public int Order { get; set; }

	/// <summary>The category the page belongs to.</summary>
	[JsonPropertyName("category")]
	public string Category { get; set; } = string.Empty;

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
