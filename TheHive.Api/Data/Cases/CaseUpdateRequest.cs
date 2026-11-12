using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of an update-case request (the spec's <c>InputUpdateCase</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
/// <remarks><see cref="CaseBulkUpdateRequest"/> derives from this class; passing a bulk request to <c>ICases.UpdateAsync</c> would also send its <c>ids</c>.</remarks>
public class CaseUpdateRequest : ClassifiedRequestBase
{
	/// <summary>The new title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	[JsonPropertyOrder(1)]
	public string? Title { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	[JsonPropertyOrder(2)]
	public string? Description { get; set; }

	/// <summary>The new incident start date.</summary>
	[JsonPropertyName("startDate")]
	[JsonPropertyOrder(110)]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>The new incident end date; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("endDate")]
	[JsonPropertyOrder(120)]
	public Optional<DateTimeOffset?> EndDate { get; set; }

	/// <summary>The name of an existing case status; a status linked to the <c>Closed</c> stage closes the case.</summary>
	[JsonPropertyName("status")]
	[JsonPropertyOrder(510)]
	public string? Status { get; set; }

	/// <summary>The new investigation summary; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("summary")]
	[JsonPropertyOrder(520)]
	public Optional<string?> Summary { get; set; }

	/// <summary>The login of the user to assign; set to <see langword="null"/> to unassign.</summary>
	[JsonPropertyName("assignee")]
	[JsonPropertyOrder(530)]
	public Optional<string?> Assignee { get; set; }

	/// <summary>The impact verdict, relevant when closing with the <c>TruePositive</c> status; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("impactStatus")]
	[JsonPropertyOrder(535)]
	public Optional<ImpactStatus?> ImpactStatus { get; set; }

	/// <summary>The custom field values; in this array form, custom fields not listed are deleted.</summary>
	[JsonPropertyName("customFields")]
	[JsonPropertyOrder(540)]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The new task-sharing rule for the owner organization.</summary>
	[JsonPropertyName("taskRule")]
	[JsonPropertyOrder(590)]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The new observable-sharing rule for the owner organization.</summary>
	[JsonPropertyName("observableRule")]
	[JsonPropertyOrder(600)]
	public SharingRule? ObservableRule { get; set; }

	/// <summary>Tags to add to the current set.</summary>
	[JsonPropertyName("addTags")]
	[JsonPropertyOrder(610)]
	public List<string>? AddTags { get; set; }

	/// <summary>Tags to remove from the current set; absent tags are ignored.</summary>
	[JsonPropertyName("removeTags")]
	[JsonPropertyOrder(620)]
	public List<string>? RemoveTags { get; set; }
}
