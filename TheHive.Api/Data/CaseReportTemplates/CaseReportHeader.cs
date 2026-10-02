using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The header rendered at the top of each page of a report (the spec's <c>HeaderWidget</c>).</summary>
public sealed class CaseReportHeader
{
	/// <summary>The Mustache template string rendered as the header, for example <c>Incident Report - {{case.title}}</c>.</summary>
	[JsonPropertyName("template")]
	public string Template { get; set; } = string.Empty;
}
