namespace TheHive.Api.Data.CaseReports;

/// <summary>
/// The wire values of the case report output formats, for <see cref="CaseReportRenderQuery.Format"/> (a query-string value). Request and response
/// bodies use the <see cref="CaseReportFormat"/> enum instead.
/// </summary>
public static class CaseReportFormats
{
	/// <summary>An HTML document.</summary>
	public const string Html = "html";

	/// <summary>A Markdown document.</summary>
	public const string Markdown = "markdown";

	/// <summary>A Word document.</summary>
	public const string Word = "word";
}
