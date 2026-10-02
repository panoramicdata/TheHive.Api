using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The body of an update-template request (the spec's <c>InputUpdateCaseReportTemplate</c>). Only set properties are sent; the rest keep their values. The spec has no clearable fields.</summary>
public sealed class CaseReportTemplateUpdateRequest
{
	/// <summary>The new title.</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new group label.</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The new summary.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new layout, which replaces the current definition.</summary>
	[JsonPropertyName("definition")]
	public CaseReportTemplateDefinition? Definition { get; set; }

	/// <summary>The new version number.</summary>
	[JsonPropertyName("version")]
	public int? Version { get; set; }
}
