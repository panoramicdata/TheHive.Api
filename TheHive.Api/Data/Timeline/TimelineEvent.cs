using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Timeline;

/// <summary>An event in a case timeline (the spec's <c>OutputTimelineEvent</c>).</summary>
public sealed class TimelineEvent
{
	/// <summary>When the event started.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset Date { get; set; }

	/// <summary>The event type.</summary>
	[JsonPropertyName("kind")]
	public TimelineEventKind Kind { get; set; }

	/// <summary>The type of the entity involved.</summary>
	[JsonPropertyName("entity")]
	public TimelineEntityType Entity { get; set; }

	/// <summary>The identifier of the entity involved.</summary>
	[JsonPropertyName("entityId")]
	public string EntityId { get; set; } = string.Empty;

	/// <summary>Event-specific details; for entity events, the entity under a key named for its type (for example <c>task</c>). Empty for case lifecycle events.</summary>
	[JsonPropertyName("details")]
	public Dictionary<string, JsonElement> Details { get; set; } = [];

	/// <summary>When a range event (such as a task) ended; <see langword="null"/> for point-in-time events.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }
}
