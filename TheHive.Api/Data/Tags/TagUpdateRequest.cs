using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Tags;

/// <summary>The body of an update-tag request (the spec's <c>InputUpdateTag</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class TagUpdateRequest
{
	/// <summary>The new tag name (1 to 128 characters); it must not duplicate a name from any existing taxonomy.</summary>
	[JsonPropertyName("predicate")]
	public string? Predicate { get; set; }

	/// <summary>The new description (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new colour, a six-digit hex colour preceded by <c>#</c> (for example <c>#e8560a</c>).</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }
}
