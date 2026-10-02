using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The footer rendered at the bottom of each page of a report (the spec's <c>FooterWidget</c>).</summary>
public sealed class CaseReportFooter
{
	/// <summary>The Mustache template string rendered as the footer, for example <c>Confidential - {{case.title}}</c>.</summary>
	[JsonPropertyName("template")]
	public string Template { get; set; } = string.Empty;
}
