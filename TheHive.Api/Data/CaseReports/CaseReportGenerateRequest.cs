using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReports;

/// <summary>The body of a generate-report request (the spec's <c>InputGenerateCaseReport</c>).</summary>
public sealed class CaseReportGenerateRequest
{
	/// <summary>The ID (preceded by <c>~</c>) of an existing case report template; list templates with the query API <c>listCaseReportTemplate</c>.</summary>
	[JsonPropertyName("caseReportTemplateId")]
	public required string CaseReportTemplateId { get; set; }

	/// <summary>The output format.</summary>
	[JsonPropertyName("format")]
	public required CaseReportFormat Format { get; set; }
}
