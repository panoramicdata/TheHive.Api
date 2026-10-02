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

	/// <summary>The severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>. The server default is 2.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>When the incident started. The server default is now.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>When the incident ended.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The tags; unknown tags are created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether to flag the case.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>. The server default is 2.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>. The server default is 2.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The name of an existing case status. The server default is <c>New</c>.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The summary of the investigation findings.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The login of the user to assign; unassigned when omitted.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The access control settings; the organization default applies when omitted.</summary>
	[JsonPropertyName("access")]
	public Access? Access { get; set; }

	/// <summary>The custom field values to set.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The name or ID of a case template to apply.</summary>
	[JsonPropertyName("caseTemplate")]
	public string? CaseTemplate { get; set; }

	/// <summary>The tasks to create; the template's tasks apply when omitted.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>The pages to create; the template's pages apply when omitted.</summary>
	[JsonPropertyName("pages")]
	public List<PageCreateRequest>? Pages { get; set; }

	/// <summary>Per-organization sharing settings that override the organization-level sharing rules.</summary>
	[JsonPropertyName("sharingParameters")]
	public List<ShareSettings>? SharingParameters { get; set; }

	/// <summary>The task-sharing rule for the owner organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The observable-sharing rule for the owner organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }
}
