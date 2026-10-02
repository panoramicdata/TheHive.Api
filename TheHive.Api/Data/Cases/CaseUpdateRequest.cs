using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of an update-case request (the spec's <c>InputUpdateCase</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class CaseUpdateRequest
{
	/// <summary>The new title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>The new incident start date.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>The new incident end date.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>Replaces all current tags with this set.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether the case is flagged.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The new Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The new Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The name of an existing case status; a status linked to the <c>Closed</c> stage closes the case.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The new investigation summary.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The login of the user to assign.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The impact verdict, relevant when closing with the <c>TruePositive</c> status.</summary>
	[JsonPropertyName("impactStatus")]
	public ImpactStatus? ImpactStatus { get; set; }

	/// <summary>The custom field values; in this array form, custom fields not listed are deleted.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The new task-sharing rule for the owner organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The new observable-sharing rule for the owner organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }

	/// <summary>Tags to add to the current set.</summary>
	[JsonPropertyName("addTags")]
	public List<string>? AddTags { get; set; }

	/// <summary>Tags to remove from the current set; absent tags are ignored.</summary>
	[JsonPropertyName("removeTags")]
	public List<string>? RemoveTags { get; set; }
}
