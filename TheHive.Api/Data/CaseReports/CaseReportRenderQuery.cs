using Refit;

namespace TheHive.Api.Data.CaseReports;

/// <summary>
/// The query-string parameters of <c>ICaseReports.RenderTemplateAsync</c>, which renders a saved case report template without saving the result.
/// Each property is sent as the query parameter of the same wire name; the optional ones are left out when <see langword="null"/>.
/// </summary>
public sealed class CaseReportRenderQuery
{
	/// <summary>
	/// The output format, one of the <see cref="CaseReportFormats"/> constants (<c>html</c>, <c>markdown</c> or <c>word</c>), sent as <c>format</c>. A string, not
	/// <see cref="CaseReportFormat"/>, because Refit writes an enum in a query string by its C# name, not by its wire value.
	/// </summary>
	[AliasAs("format")]
	public required string Format { get; set; }

	/// <summary>The ID (preceded by <c>~</c>) of the saved template, sent as <c>caseReportTemplateId</c>.</summary>
	[AliasAs("caseReportTemplateId")]
	public required string CaseReportTemplateId { get; set; }

	/// <summary>The ID (preceded by <c>~</c>) of the case whose data fills the report, sent as <c>caseId</c>; when <see langword="null"/> it is left out and the server uses fake data.</summary>
	[AliasAs("caseId")]
	public string? CaseId { get; set; }

	/// <summary>The maximum number of elements per widget, sent as <c>maxElements</c>; left out when <see langword="null"/>.</summary>
	[AliasAs("maxElements")]
	public int? MaxElements { get; set; }
}
