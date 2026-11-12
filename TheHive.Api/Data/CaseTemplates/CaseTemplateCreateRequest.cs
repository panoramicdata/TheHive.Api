using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.CaseTemplates;

/// <summary>
/// The body of a create-case-template request (the spec's <c>InputCreateCaseTemplate</c>). Unset properties are omitted.
/// The deprecated <c>pageTemplateIds</c> property is not modelled: use <see cref="PageTemplates"/>.
/// </summary>
public sealed class CaseTemplateCreateRequest : ClassifiedRequestBase
{
	/// <summary>The name of the template (1 to 128 characters); unique in the organization.</summary>
	[JsonPropertyName("name")]
	[JsonPropertyOrder(1)]
	public required string Name { get; set; }

	/// <summary>The name shown to users when selecting the template (1 to 128 characters); the server defaults it to <see cref="Name"/>.</summary>
	[JsonPropertyName("displayName")]
	[JsonPropertyOrder(2)]
	public string? DisplayName { get; set; }

	/// <summary>The prefix added to case titles when the template is applied (1 to 64 characters); end it with a hyphen to separate it from the title.</summary>
	[JsonPropertyName("titlePrefix")]
	[JsonPropertyOrder(3)]
	public string? TitlePrefix { get; set; }

	/// <summary>The default case description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	[JsonPropertyOrder(4)]
	public string? Description { get; set; }

	/// <summary>The default case summary (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("summary")]
	[JsonPropertyOrder(510)]
	public string? Summary { get; set; }

	/// <summary>The task templates created on cases when the template is applied.</summary>
	[JsonPropertyName("tasks")]
	[JsonPropertyOrder(520)]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }

	/// <summary>The default custom field values; each must match an existing custom field. Only the array form is modelled.</summary>
	[JsonPropertyName("customFields")]
	[JsonPropertyOrder(530)]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The pages created on cases when the template is applied.</summary>
	[JsonPropertyName("pageTemplates")]
	[JsonPropertyOrder(540)]
	public List<PageCreateRequest>? PageTemplates { get; set; }
}
