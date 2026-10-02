using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.CaseTemplates;

/// <summary>
/// The body of a create-case-template request (the spec's <c>InputCreateCaseTemplate</c>). Unset properties are omitted.
/// The deprecated <c>pageTemplateIds</c> property is not modelled: use <see cref="PageTemplates"/>.
/// </summary>
public sealed class CaseTemplateCreateRequest
{
	/// <summary>The name of the template (1 to 128 characters); unique in the organization.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The name shown to users when selecting the template (1 to 128 characters); the server defaults it to <see cref="Name"/>.</summary>
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	/// <summary>The prefix added to case titles when the template is applied (1 to 64 characters); end it with a hyphen to separate it from the title.</summary>
	[JsonPropertyName("titlePrefix")]
	public string? TitlePrefix { get; set; }

	/// <summary>The default case description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The default severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>The default tags; unknown tags are created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether cases are flagged as important by default.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The default TLP level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The default PAP level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The default case summary (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The task templates created on cases when the template is applied.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>The default custom field values; each must match an existing custom field. Only the array form is modelled.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The pages created on cases when the template is applied.</summary>
	[JsonPropertyName("pageTemplates")]
	public List<PageCreateRequest>? PageTemplates { get; set; }
}
