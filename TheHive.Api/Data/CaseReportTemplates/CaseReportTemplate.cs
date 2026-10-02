using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>A case report template (the spec's <c>OutputCaseReportTemplate</c>).</summary>
public sealed class CaseReportTemplate
{
	/// <summary>The internal identifier (for example <c>~42123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, <c>caseReportTemplate</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the template.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the template, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the template was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the template was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

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
