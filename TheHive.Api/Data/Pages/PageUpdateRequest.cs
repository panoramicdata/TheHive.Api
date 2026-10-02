using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Pages;

/// <summary>The body of an update-page request (the spec's <c>InputUpdatePage</c>), also used for page templates. Only set properties are sent; the rest keep their values.</summary>
public sealed class PageUpdateRequest
{
	/// <summary>The new title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new content (TheHive-flavored Markdown, up to 1 MiB).</summary>
	[JsonPropertyName("content")]
	public string? Content { get; set; }

	/// <summary>The new display order within the category.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>The new category (1 to 128 characters); unknown categories are created.</summary>
	[JsonPropertyName("category")]
	public string? Category { get; set; }
}
