using System.Net;
using System.Text.Json;
using Refit;
using TheHive.Api.Data.CaseReports;
using TheHive.Api.Data.CaseReportTemplates;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CaseReportsTests
{
	private const string ReportJson = """
		{
			"_id":"~84512","_type":"CaseReport","_createdBy":"lucas@example.com","_updatedBy":"emma@example.com",
			"_createdAt":1716414900000,"_updatedAt":1716415200000,"contentType":"text/html","size":245760,
			"filename":"Case#42 Incident Report 2025-05-22.html"
		}
		""";

	private const string MinimalReportJson = """
		{
			"_id":"~1","_type":"CaseReport","_createdBy":"lucas@example.com","_createdAt":1716414900000,
			"contentType":"text/html","size":1,"filename":"r.html"
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullReport(CaseReport item)
	{
		item.Id.Should().Be("~84512");
		item.Type.Should().Be("CaseReport");
		item.CreatedBy.Should().Be("lucas@example.com");
		item.UpdatedBy.Should().Be("emma@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1716414900000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1716415200000));
		item.ContentType.Should().Be("text/html");
		item.Size.Should().Be(245760);
		item.Filename.Should().Be("Case#42 Incident Report 2025-05-22.html");
	}

	[Theory]
	[InlineData(CaseReportFormat.Html, "html")]
	[InlineData(CaseReportFormat.Markdown, "markdown")]
	[InlineData(CaseReportFormat.Word, "word")]
	public async Task GenerateAsync_PostsBodyAndMapsEveryField(CaseReportFormat format, string wire)
	{
		var stub = Stub(HttpStatusCode.Created, ReportJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseReports.GenerateAsync(
			"~123",
			new CaseReportGenerateRequest { CaseReportTemplateId = "~84512", Format = format },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/report");
		stub.Calls[0].Body.Should().Be($$"""{"caseReportTemplateId":"~84512","format":"{{wire}}"}""");
		AssertFullReport(result);
	}

	[Fact]
	public async Task GenerateAsync_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalReportJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseReports.GenerateAsync(
			"~123",
			new CaseReportGenerateRequest { CaseReportTemplateId = "~84512", Format = CaseReportFormat.Html },
			TestContext.Current.CancellationToken);

		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
	}

	[Fact]
	public void CaseReport_Defaults_AreEmptyNotNull()
	{
		var item = new CaseReport();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.ContentType.Should().BeEmpty();
		item.Filename.Should().BeEmpty();
	}

	[Fact]
	public async Task UploadAsync_SendsFilePart_And_MapsResult()
	{
		var stub = Stub(HttpStatusCode.Created, ReportJson);
		using var client = TestClient.Create(stub);
		using var file = new MemoryStream([0x25, 0x50, 0x44, 0x46]);

		var result = await client.CaseReports.UploadAsync("~123", new StreamPart(file, "report.pdf", "application/pdf"), TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/case/~123/report/upload");
		call.ContentType.Should().Be("multipart/form-data");
		var part = call.Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("file");
		part.FileName.Should().Be("report.pdf");
		part.ContentType.Should().Be("application/pdf");
		part.Bytes.Should().Equal(0x25, 0x50, 0x44, 0x46);
		AssertFullReport(result);
	}

	[Fact]
	public async Task UpdateAsync_PatchesFilePart()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReports.UpdateAsync("~84512", new ByteArrayPart([7, 8, 9], "new.html", "text/html"), TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Patch);
		call.Uri.AbsolutePath.Should().Be("/api/v1/caseReport/~84512");
		call.ContentType.Should().Be("multipart/form-data");
		var part = call.Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("file");
		part.FileName.Should().Be("new.html");
		part.ContentType.Should().Be("text/html");
		part.Bytes.Should().Equal(7, 8, 9);
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReports.DeleteAsync("~84512", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReport/~84512");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DownloadAsync_ReturnsExactBytesAndFileName()
	{
		var stub = new StubHandler();
		byte[] bytes = [0x3C, 0x68, 0x74, 0x6D, 0x6C, 0x00, 0xFF];
		stub.EnqueueFile(bytes, "text/html", "Case#42 Incident Report.html");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.DownloadAsync("~84512", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReport/~84512/download");
		content.Headers.ContentType!.MediaType.Should().Be("text/html");
		content.Headers.ContentDisposition!.FileName.Should().Be("Case#42 Incident Report.html");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(bytes);
	}

	[Fact]
	public async Task ViewAsync_ReturnsExactBytes()
	{
		var stub = new StubHandler();
		byte[] bytes = [0x3C, 0x70, 0x3E, 0x00, 0xFE];
		stub.EnqueueFile(bytes, "text/html", "r.html");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.ViewAsync("~84512", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReport/~84512/view");
		content.Headers.ContentType!.MediaType.Should().Be("text/html");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(bytes);
	}

	[Fact]
	public async Task RenderTemplateAsync_SendsQueryParametersAndReturnsBytes()
	{
		var stub = new StubHandler();
		byte[] bytes = [1, 2, 3];
		stub.EnqueueFile(bytes, "text/html", "preview.html");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.RenderTemplateAsync(
			new CaseReportRenderQuery { Format = CaseReportFormats.Html, CaseReportTemplateId = "~84512", CaseId = "~59643", MaxElements = 5 },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReport/render");
		stub.Calls[0].Uri.Query.Should().Be("?format=html&caseReportTemplateId=~84512&caseId=~59643&maxElements=5");
		stub.Calls[0].Body.Should().BeNull();
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(bytes);
	}

	[Fact]
	public async Task RenderTemplateAsync_RequiredOnly_OmitsOptionalQuery()
	{
		var stub = new StubHandler();
		stub.EnqueueFile([1], "text/markdown", "preview.md");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.RenderTemplateAsync(
			new CaseReportRenderQuery { Format = CaseReportFormats.Markdown, CaseReportTemplateId = "~84512" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?format=markdown&caseReportTemplateId=~84512");
	}

	[Theory]
	[InlineData(CaseReportFormats.Html, CaseReportFormat.Html)]
	[InlineData(CaseReportFormats.Markdown, CaseReportFormat.Markdown)]
	[InlineData(CaseReportFormats.Word, CaseReportFormat.Word)]
	public void CaseReportFormats_MatchTheEnumWireValues(string constant, CaseReportFormat format) =>
		JsonSerializer.Serialize(format, TheHiveJson.Options).Should().Be($"\"{constant}\"");

	[Fact]
	public async Task RenderAsync_PostsInlineDefinition_And_ReturnsBytes()
	{
		var stub = new StubHandler();
		byte[] bytes = [4, 5, 6];
		stub.EnqueueFile(bytes, "application/octet-stream", "preview.docx");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.RenderAsync(
			new CaseReportRenderRequest
			{
				Format = CaseReportFormat.Word,
				Definition = new CaseReportTemplateDefinition
				{
					Widgets = [new CaseReportWidget { Kind = CaseReportWidgetKinds.Text, Template = "Hello" }]
				},
				CaseReportTemplateId = "~84512",
				CaseId = "~59643",
				MaxElements = 5
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReport/render");
		stub.Calls[0].Body.Should().Be(
			"""{"format":"word","definition":{"widgets":[{"_kind":"Text","template":"Hello"}],"i18n":{}},"caseReportTemplateId":"~84512","caseId":"~59643","maxElements":5}""");
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(bytes);
	}

	[Fact]
	public async Task RenderAsync_FormatOnly_OmitsOptionals()
	{
		var stub = new StubHandler();
		stub.EnqueueFile([1], "text/html", "preview.html");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReports.RenderAsync(
			new CaseReportRenderRequest { Format = CaseReportFormat.Html, CaseReportTemplateId = "~84512" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"format":"html","caseReportTemplateId":"~84512"}""");
	}

	[Fact]
	public async Task DownloadAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"CaseReport ~1 not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.CaseReports.DownloadAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
