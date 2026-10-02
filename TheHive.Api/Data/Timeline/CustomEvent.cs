using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Timeline;

/// <summary>A custom event added to a case timeline (the spec's <c>OutputCustomEvent</c>).</summary>
public sealed class CustomEvent
{
	/// <summary>The internal identifier (for example <c>~24568324</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>CustomEvent</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the event.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the event, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the event was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the event was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

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
