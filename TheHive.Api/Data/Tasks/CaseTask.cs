using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Tasks;

/// <summary>A task in a case (the spec's <c>OutputTask</c>).</summary>
public sealed class CaseTask
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Task</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the task.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the task, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the task was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the task was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The group used to organize the task within the case.</summary>
	[JsonPropertyName("group")]
	public string Group { get; set; } = string.Empty;

	/// <summary>The description (TheHive-flavored Markdown), if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The current status.</summary>
	[JsonPropertyName("status")]
	public CaseTaskStatus Status { get; set; }

	/// <summary>Whether the task is flagged as important.</summary>
	[JsonPropertyName("flag")]
	public bool Flag { get; set; }

	/// <summary>When the task started, if it has.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>When the task ended, if it has.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The login of the user assigned to the task, if any.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The display order within the group.</summary>
	[JsonPropertyName("order")]
	public int Order { get; set; }

	/// <summary>When the task is due, if a due date is set.</summary>
	[JsonPropertyName("dueDate")]
	public DateTimeOffset? DueDate { get; set; }

	/// <summary>Whether the task must be completed before the case can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool Mandatory { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>The duration of the task from start to completion, in milliseconds, if known.</summary>
	[JsonPropertyName("timeToHandle")]
	public long? TimeToHandle { get; set; }
}
