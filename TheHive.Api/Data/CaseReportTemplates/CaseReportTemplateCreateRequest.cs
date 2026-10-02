using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>A case report template to create (the spec's <c>InputCreateCaseReportTemplate</c>). Unset optional properties are omitted.</summary>
public sealed class CaseReportTemplateCreateRequest
{
	/// <summary>The title, as displayed when selecting reports from case descriptions.</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The group label used to categorize the template.</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>A brief summary of the template's purpose and contents.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The layout: widgets, header, footer, date formats and language.</summary>
	[JsonPropertyName("definition")]
	public required CaseReportTemplateDefinition Definition { get; set; }

	/// <summary>The version number of the template.</summary>
	[JsonPropertyName("version")]
	public int? Version { get; set; }
}
