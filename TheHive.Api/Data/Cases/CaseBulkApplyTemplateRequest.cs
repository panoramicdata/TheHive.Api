using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of an apply-case-template request (the spec's <c>InputApplyCaseTemplateWithIds</c>). Unset options are omitted; the server default for each is <see langword="false"/>.</summary>
public sealed class CaseBulkApplyTemplateRequest
{
	/// <summary>The cases to update: IDs preceded by <c>~</c>, or case numbers.</summary>
	[JsonPropertyName("ids")]
	public required List<string> Ids { get; set; }

	/// <summary>The name or ID of the case template to apply.</summary>
	[JsonPropertyName("caseTemplate")]
	public required string CaseTemplate { get; set; }

	/// <summary>Whether to apply the template's title prefix.</summary>
	[JsonPropertyName("updateTitlePrefix")]
	public bool? UpdateTitlePrefix { get; set; }

	/// <summary>Whether to replace each case's description with the template's.</summary>
	[JsonPropertyName("updateDescription")]
	public bool? UpdateDescription { get; set; }

	/// <summary>Whether to apply the template's tags.</summary>
	[JsonPropertyName("updateTags")]
	public bool? UpdateTags { get; set; }

	/// <summary>Whether to apply the template's severity.</summary>
	[JsonPropertyName("updateSeverity")]
	public bool? UpdateSeverity { get; set; }

	/// <summary>Whether to apply the template's flag.</summary>
	[JsonPropertyName("updateFlag")]
	public bool? UpdateFlag { get; set; }

	/// <summary>Whether to apply the template's Traffic Light Protocol level.</summary>
	[JsonPropertyName("updateTlp")]
	public bool? UpdateTlp { get; set; }

	/// <summary>Whether to apply the template's Permissible Actions Protocol level.</summary>
	[JsonPropertyName("updatePap")]
	public bool? UpdatePap { get; set; }

	/// <summary>Whether to add the template's custom field values to each case, keeping existing values.</summary>
	[JsonPropertyName("updateCustomFields")]
	public bool? UpdateCustomFields { get; set; }

	/// <summary>The IDs or titles of template tasks to import into each case.</summary>
	[JsonPropertyName("importTasks")]
	public List<string>? ImportTasks { get; set; }

	/// <summary>The IDs of template pages to import into each case.</summary>
	[JsonPropertyName("importPages")]
	public List<string>? ImportPages { get; set; }
}
