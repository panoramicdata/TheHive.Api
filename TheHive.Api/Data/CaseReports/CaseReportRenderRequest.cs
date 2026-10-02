using System.Text.Json.Serialization;
using TheHive.Api.Data.CaseReportTemplates;

namespace TheHive.Api.Data.CaseReports;

/// <summary>The body of a render-report (preview) request (the spec's <c>InputRenderCaseReport</c>). Set either <see cref="Definition"/> or <see cref="CaseReportTemplateId"/>; the server requires one of them.</summary>
public sealed class CaseReportRenderRequest
{
	/// <summary>The output format.</summary>
	[JsonPropertyName("format")]
	public required CaseReportFormat Format { get; set; }

	/// <summary>An inline template definition, for rendering without a saved template.</summary>
	[JsonPropertyName("definition")]
	public CaseReportTemplateDefinition? Definition { get; set; }

	/// <summary>The ID (preceded by <c>~</c>) of the saved case report template to use.</summary>
	[JsonPropertyName("caseReportTemplateId")]
	public string? CaseReportTemplateId { get; set; }

	/// <summary>The ID (preceded by <c>~</c>) of the case whose data fills the report; when omitted the server uses fake data.</summary>
	[JsonPropertyName("caseId")]
	public string? CaseId { get; set; }

	/// <summary>The maximum number of elements displayed per widget in the preview (does not affect saved or downloaded reports).</summary>
	[JsonPropertyName("maxElements")]
	public int? MaxElements { get; set; }
}
