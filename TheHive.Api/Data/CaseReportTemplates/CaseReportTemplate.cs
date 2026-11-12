using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>A case report template (the spec's <c>OutputCaseReportTemplate</c>).</summary>
public sealed class CaseReportTemplate : TheHiveEntity
{
	/// <summary>The title, as displayed when selecting reports from case descriptions.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The group label used to categorize the template.</summary>
	[JsonPropertyName("group")]
	public string Group { get; set; } = string.Empty;

	/// <summary>The brief summary of the template's purpose and contents.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The version number of the template.</summary>
	[JsonPropertyName("version")]
	public int Version { get; set; }

	/// <summary>The layout: widgets, header, footer, date formats and language.</summary>
	[JsonPropertyName("definition")]
	public CaseReportTemplateDefinition Definition { get; set; } = new();
}
