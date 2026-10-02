using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Pages;

/// <summary>A page to create (the spec's <c>InputCreatePage</c>). Unset properties are omitted.</summary>
public sealed class PageCreateRequest
{
	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The content (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("content")]
	public required string Content { get; set; }

	/// <summary>The display order within its category.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>The category; unknown categories are created.</summary>
	[JsonPropertyName("category")]
	public required string Category { get; set; }
}
