using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Cases;

namespace TheHive.Api.Data.CaseStatuses;

/// <summary>A configurable case status (the spec's <c>OutputCaseStatus</c>).</summary>
public sealed class CaseStatus
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The object type, always <c>CaseStatus</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>When the status was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login of the user who last updated the status, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the status was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who created the status.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The display name of the status (for example <c>Contained</c>); it is what a case's status holds.</summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;

	/// <summary>The stage the status belongs to (the spec types it as a string with the values of <c>InputCaseStage</c>).</summary>
	[JsonPropertyName("stage")]
	public CaseStage Stage { get; set; }

	/// <summary>The position of the status in the UI dropdown list, if set.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>A human-readable description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour used to display the status in the UI (for example <c>#ffa940</c>), if set.</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }

	/// <summary>Whether the status is hidden from the UI dropdown; hidden statuses stay usable through the API.</summary>
	[JsonPropertyName("hidden")]
	public bool Hidden { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
