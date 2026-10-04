using Refit;
using TheHive.Api.Data.CaseReports;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on case reports: report files generated from a case report template or uploaded, attached to a case, and previews
/// rendered without saving. To list reports use the query API with <c>listCaseReport</c>. Requires a Platinum licence; writes require
/// <c>manageCaseReport</c>. Templates are on <see cref="ICaseReportTemplates"/>.
/// </summary>
/// <remarks>
/// Every method that returns <see cref="HttpContent"/> hands the response to the caller, who owns it and must dispose it.
/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
/// Rendered output (<see cref="RenderTemplateAsync"/>, <see cref="RenderAsync"/>, <see cref="ViewAsync"/>) is meant to be shown in a
/// browser: the server sends <c>Content-Security-Policy</c> and <c>Referrer-Policy</c> headers that you should keep if you serve it on.
/// </remarks>
public interface ICaseReports
{
	/// <summary>Generates a report for a case from a saved template and attaches it to the case (requires <c>manageCaseReport</c>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The template and output format.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The report created.</returns>
	[Post("api/v1/case/{caseId}/report")]
	Task<CaseReport> GenerateAsync(string caseId, [Body] CaseReportGenerateRequest request, CancellationToken cancellationToken);

	/// <summary>Attaches an externally produced report file to a case (requires <c>manageCaseReport</c>).</summary>
	/// <remarks>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>), so a file is never stored twice; the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the file, so raise it for large uploads.</remarks>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="file">The report file, sent as the multipart part named <c>file</c>. Build it with a file name and, ideally, a content type,
	/// for example <c>new StreamPart(stream, "report.pdf", "application/pdf")</c>; leave the part name unset.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The report created.</returns>
	[Multipart]
	[Post("api/v1/case/{caseId}/report/upload")]
	Task<CaseReport> UploadAsync(string caseId, [AliasAs("file")] MultipartItem file, CancellationToken cancellationToken);

	/// <summary>Replaces the file of a case report (requires <c>manageCaseReport</c>).</summary>
	/// <remarks>Uploads are never retried; see <see cref="UploadAsync"/>.</remarks>
	/// <param name="reportId">The case report ID preceded by <c>~</c>.</param>
	/// <param name="file">The new report file, sent as the multipart part named <c>file</c>; see <see cref="UploadAsync"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Multipart]
	[Patch("api/v1/caseReport/{reportId}")]
	Task UpdateAsync(string reportId, [AliasAs("file")] MultipartItem file, CancellationToken cancellationToken);

	/// <summary>Deletes a case report (requires <c>manageCaseReport</c>).</summary>
	/// <param name="reportId">The case report ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/caseReport/{reportId}")]
	Task DeleteAsync(string reportId, CancellationToken cancellationToken);

	/// <summary>Downloads a case report file as an attachment.</summary>
	/// <param name="reportId">The case report ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The file; the suggested name is in <c>Headers.ContentDisposition.FileName</c>. The caller owns the content and must dispose it.</returns>
	[Get("api/v1/caseReport/{reportId}/download")]
	Task<HttpContent> DownloadAsync(string reportId, CancellationToken cancellationToken);

	/// <summary>Gets the content of a case report for inline display in a browser.</summary>
	/// <param name="reportId">The case report ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The report content. The caller owns the content and must dispose it.</returns>
	[Get("api/v1/caseReport/{reportId}/view")]
	Task<HttpContent> ViewAsync(string reportId, CancellationToken cancellationToken);

	/// <summary>Renders a preview of a case report from a saved template without saving it. Use <see cref="RenderAsync"/> for an inline definition.</summary>
	/// <param name="query">The format, template and optional case and element limit (<c>format</c>, <c>caseReportTemplateId</c>, <c>caseId</c>, <c>maxElements</c>). Pass a non-null query: a <see langword="null"/> one is not rejected by the client, it sends no query parameters and the server rejects the request.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The rendered output. The caller owns the content and must dispose it.</returns>
	[Get("api/v1/caseReport/render")]
	Task<HttpContent> RenderTemplateAsync([Query] CaseReportRenderQuery query, CancellationToken cancellationToken);

	/// <summary>Renders a preview of a case report from a saved template or an inline definition, without saving it.</summary>
	/// <param name="request">What to render.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The rendered output. The caller owns the content and must dispose it.</returns>
	[Post("api/v1/caseReport/render")]
	Task<HttpContent> RenderAsync([Body] CaseReportRenderRequest request, CancellationToken cancellationToken);
}
