using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Timeline;

/// <summary>A custom event added to a case timeline (the spec's <c>OutputCustomEvent</c>).</summary>
public sealed class CustomEvent : TheHiveEntity
{
	/// <summary>When the event started.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset Date { get; set; }

	/// <summary>When the event ended, if it is a range event.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The title, visible on the timeline.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The brief description, visible in the timeline preview.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
