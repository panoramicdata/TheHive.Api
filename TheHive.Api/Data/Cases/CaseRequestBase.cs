using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.Cases;

/// <summary>The properties shared by the create-case and create-case-from-alert request bodies. Unset properties are omitted.</summary>
public abstract class CaseRequestBase : ClassifiedRequestBase
{
	/// <summary>Incident start date; when omitted the server stamps it with now.</summary>
	[JsonPropertyName("startDate")]
	[JsonPropertyOrder(110)]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>The date the incident ended.</summary>
	[JsonPropertyName("endDate")]
	[JsonPropertyOrder(120)]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>Existing case status to use; defaults to <c>New</c>.</summary>
	[JsonPropertyName("status")]
	[JsonPropertyOrder(510)]
	public string? Status { get; set; }

	/// <summary>Investigation findings in a summary.</summary>
	[JsonPropertyName("summary")]
	[JsonPropertyOrder(520)]
	public string? Summary { get; set; }

	/// <summary>Login of the assignee; leave out for no owner.</summary>
	[JsonPropertyName("assignee")]
	[JsonPropertyOrder(530)]
	public string? Assignee { get; set; }
	/// <summary>Values for the custom fields to be created.</summary>
	[JsonPropertyName("customFields")]
	[JsonPropertyOrder(540)]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>Case template to apply, referenced by name or ID.</summary>
	[JsonPropertyName("caseTemplate")]
	[JsonPropertyOrder(550)]
	public string? CaseTemplate { get; set; }

	/// <summary>Tasks to create alongside the case; absent means the template's tasks are used.</summary>
	[JsonPropertyName("tasks")]
	[JsonPropertyOrder(560)]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>Pages to create alongside the case; absent means the template's pages are used.</summary>
	[JsonPropertyName("pages")]
	[JsonPropertyOrder(570)]
	public List<PageCreateRequest>? Pages { get; set; }

	/// <summary>Org-specific sharing settings that take precedence over the global sharing rules.</summary>
	[JsonPropertyName("sharingParameters")]
	[JsonPropertyOrder(580)]
	public List<ShareSettings>? SharingParameters { get; set; }

	/// <summary>Sharing rule for tasks of the owner org.</summary>
	[JsonPropertyName("taskRule")]
	[JsonPropertyOrder(590)]
	public SharingRule? TaskRule { get; set; }

	/// <summary>Sharing rule for observables of the owner org.</summary>
	[JsonPropertyName("observableRule")]
	[JsonPropertyOrder(600)]
	public SharingRule? ObservableRule { get; set; }
}

