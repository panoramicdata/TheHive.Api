using System.Text.Json.Serialization;
using TheHive.Api.Data.Alerts;

namespace TheHive.Api.Data.AlertStatuses;

/// <summary>The body of a create-alert-status request (the spec's <c>InputCreateAlertStatus</c>). Unset properties are omitted. <see cref="Value"/> and <see cref="Stage"/> cannot be changed after creation.</summary>
public sealed class AlertStatusCreateRequest
{
	/// <summary>The display name of the status (1 to 64 characters); immutable after creation.</summary>
	[JsonPropertyName("value")]
	public required string Value { get; set; }

	/// <summary>The stage the status belongs to; immutable after creation.</summary>
	[JsonPropertyName("stage")]
	public required AlertStage Stage { get; set; }

	/// <summary>The position of the status in the UI dropdown list; lower values appear first.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>A human-readable description (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour used to display the status in the UI (for example <c>#52c41a</c>).</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }

	/// <summary>Whether to hide the status from the UI dropdown; hidden statuses stay usable through the API.</summary>
	[JsonPropertyName("hidden")]
	public bool? Hidden { get; set; }
}
