using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Timeline;

/// <summary>A custom event to add to a case timeline (the spec's <c>InputCustomEvent</c>). Unset properties are omitted.</summary>
public sealed class CustomEventCreateRequest
{
	/// <summary>When the event started.</summary>
	[JsonPropertyName("date")]
	public required DateTimeOffset Date { get; set; }

	/// <summary>When the event ended, for a range event.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The title, visible on the timeline (1 to 128 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>A brief description, visible in the timeline preview.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
