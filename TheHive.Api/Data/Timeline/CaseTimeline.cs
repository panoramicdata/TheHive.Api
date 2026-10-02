using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Timeline;

/// <summary>The activity timeline of a case (the spec's <c>OutputTimeline</c>).</summary>
public sealed class CaseTimeline
{
	/// <summary>The events, sorted by date.</summary>
	[JsonPropertyName("events")]
	public List<TimelineEvent> Events { get; set; } = [];
}
