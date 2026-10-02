using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Tasks;

/// <summary>A task to create (the spec's <c>InputCreateTask</c>). Unset properties are omitted.</summary>
public sealed class CaseTaskCreateRequest
{
	/// <summary>The title (1 to 128 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The group used to organize tasks within the case.</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The initial status.</summary>
	[JsonPropertyName("status")]
	public CaseTaskStatus? Status { get; set; }

	/// <summary>Whether to flag the task.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>When the task started.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>When the task ended.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The display order within its group.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>When the task is due.</summary>
	[JsonPropertyName("dueDate")]
	public DateTimeOffset? DueDate { get; set; }

	/// <summary>The login (email address) of the user to assign.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>Whether the task must be completed before the case can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool? Mandatory { get; set; }
}
