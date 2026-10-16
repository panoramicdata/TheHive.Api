using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of a create-case request (the spec's <c>InputCreateCase</c>). Unset properties are omitted.</summary>
public sealed class CaseCreateRequest
{
	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The severity (1 = low, 4 = critical; see <see cref="Common.Severity"/>). Server default: 2.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>Incident start date; when omitted the server stamps it with now.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>The date the incident ended.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>List of tags; any unknown tag gets created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Flag the case.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>Traffic Light Protocol level 0–4 (<see cref="Common.Tlp"/>). Server default: 2.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>Permissible Actions Protocol level 0–3 (<see cref="Common.Pap"/>). Server default: 2.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>Existing case status to use; defaults to <c>New</c>.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>Investigation findings in a summary.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>Login of the assignee; leave out for no owner.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>Access-control settings; the organization's default is used when not given.</summary>
	[JsonPropertyName("access")]
	public Access? Access { get; set; }

	/// <summary>Values for the custom fields to be created.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>Case template to apply, referenced by name or ID.</summary>
	[JsonPropertyName("caseTemplate")]
	public string? CaseTemplate { get; set; }

	/// <summary>Tasks to create alongside the case; absent means the template's tasks are used.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>Pages to create alongside the case; absent means the template's pages are used.</summary>
	[JsonPropertyName("pages")]
	public List<PageCreateRequest>? Pages { get; set; }

	/// <summary>Org-specific sharing settings that take precedence over the global sharing rules.</summary>
	[JsonPropertyName("sharingParameters")]
	public List<ShareSettings>? SharingParameters { get; set; }

	/// <summary>Sharing rule for tasks of the owner org.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>Sharing rule for observables of the owner org.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }
}
