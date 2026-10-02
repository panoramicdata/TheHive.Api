using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.Misp;

/// <summary>The settings sent as the <c>_json</c> part when importing a MISP event as a case (the spec's <c>InputImportCase</c>). Unset properties are omitted.</summary>
public sealed class MispCaseImportRequest
{
	/// <summary>The name or ID of a case template to apply.</summary>
	[JsonPropertyName("caseTemplate")]
	public string? CaseTemplate { get; set; }

	/// <summary>The login of the user to assign; unassigned when omitted.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The tasks to create.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>The pages to create.</summary>
	[JsonPropertyName("pages")]
	public List<PageCreateRequest>? Pages { get; set; }

	/// <summary>The custom field values to set, in the array form (the spec's object form is not modelled).</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

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
