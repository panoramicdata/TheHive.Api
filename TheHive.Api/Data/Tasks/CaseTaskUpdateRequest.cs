using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Tasks;

/// <summary>The body of an update-task request (the spec's <c>InputUpdateTask</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
/// <remarks><see cref="CaseTaskBulkUpdateRequest"/> derives from this class; passing a bulk request to <c>ITasks.UpdateAsync</c> would also send its <c>ids</c>.</remarks>
public class CaseTaskUpdateRequest
{
	/// <summary>The new title (1 to 128 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new group (1 to 64 characters).</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown); set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("description")]
	public Optional<string?> Description { get; set; }

	/// <summary>The new status.</summary>
	[JsonPropertyName("status")]
	public CaseTaskStatus? Status { get; set; }

	/// <summary>Whether the task is flagged as important.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The new start date; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("startDate")]
	public Optional<DateTimeOffset?> StartDate { get; set; }

	/// <summary>The new end date.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The new display order within the group.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>The new due date; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("dueDate")]
	public Optional<DateTimeOffset?> DueDate { get; set; }

	/// <summary>The login of the user to assign (1 to 128 characters); set to <see langword="null"/> to unassign.</summary>
	[JsonPropertyName("assignee")]
	public Optional<string?> Assignee { get; set; }

	/// <summary>Whether the task must be completed before the case can be closed.</summary>
	[JsonPropertyName("mandatory")]
	public bool? Mandatory { get; set; }
}
