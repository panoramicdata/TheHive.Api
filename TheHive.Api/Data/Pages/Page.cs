using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Pages;

/// <summary>A Knowledge Base, case or page-template page (the spec's <c>OutputPage</c>).</summary>
public sealed class Page : TheHiveEntity
{
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
