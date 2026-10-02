using System.Net;
using Refit;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Misp;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class MispTests
{
	private const string CaseJson = """
		{
			"_id":"~123","_type":"Case","_createdBy":"alice@example.com","_createdAt":1700000000000,"number":7,
			"title":"MISP event 42","description":"d","severity":2,"severityLabel":"MEDIUM","startDate":1700000000000,
			"flag":false,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","status":"New",
			"stage":"New","access":{"_kind":"OrganisationAccessKind"},"extraData":{},
			"newDate":1700000000000,"timeToDetect":0
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task GetStatusAsync_Gets_AndReturnsTheRawJson()
	{
		var stub = Stub(HttpStatusCode.OK, """{"misp-prod":{"lastSyncDate":1700000000000,"lastEventDate":1700000001000,"partial":false}}""");
		using var client = TestClient.Create(stub);

		var status = await client.Misp.GetStatusAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/misp/status");
		stub.Calls[0].Body.Should().BeNull();
		var server = status.GetProperty("misp-prod");
		server.GetProperty("lastSyncDate").GetInt64().Should().Be(1700000000000);
		server.GetProperty("partial").GetBoolean().Should().BeFalse();
	}

	[Fact]
	public async Task SyncAlertsAsync_SendsGetWithoutBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Misp.SyncAlertsAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/misp/_syncAlerts");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ExportCaseAsync_PostsWithEscapedSegments()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Misp.ExportCaseAsync("~123", "misp prod", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/misp/export/~123/misp%20prod");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ImportCaseAsync_UploadsJsonAndFileParts_AndMapsTheCase()
	{
		var stub = Stub(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);
		byte[] eventFile = [0x7B, 0x7D, 0x0A];
		var request = new MispCaseImportRequest
		{
			CaseTemplate = "phishing",
			Assignee = "bob@example.com",
			Tasks = [new CaseTaskCreateRequest { Title = "Triage" }],
			Pages = [new PageCreateRequest { Title = "Notes", Content = "text", Order = 1, Category = "general" }],
			CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware", Order = 0 }],
			SharingParameters = [new ShareSettings { Organisation = "Org" }],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.AutoShare
		};

		var result = await client.Misp.ImportCaseAsync(request, new ByteArrayPart(eventFile, "event.json", "application/json"), TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/connector/misp/case/import");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Name.Should().Be("_json");
		call.Parts[0].ContentType.Should().Be("application/json");
		call.Parts[0].Text.Should().Be(
			"""{"caseTemplate":"phishing","assignee":"bob@example.com","tasks":[{"title":"Triage"}],"pages":[{"title":"Notes","content":"text","order":1,"category":"general"}],"customFields":[{"name":"threat-type","value":"Malware","order":0}],"sharingParameters":[{"organisation":"Org"}],"taskRule":"manual","observableRule":"autoShare"}""");
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "file" && p.FileName == "event.json" && p.ContentType == "application/json");
		call.Parts[1].Bytes.Should().Equal(eventFile);
		result.Id.Should().Be("~123");
		result.Title.Should().Be("MISP event 42");
	}

	[Fact]
	public async Task ImportCaseAsync_EmptyRequest_SendsAnEmptyJsonObject()
	{
		var stub = Stub(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);

		await client.Misp.ImportCaseAsync(new MispCaseImportRequest(), new ByteArrayPart([1], "e.json"), TestContext.Current.CancellationToken);

		stub.Calls[0].Parts[0].Text.Should().Be("{}");
	}

	[Fact]
	public async Task ImportCaseAsync_NonSeekableStream_IsUploadedOnce()
	{
		var stub = Stub(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);
		using var stream = new NonSeekableStream([7, 8, 9]);

		await client.Misp.ImportCaseAsync(new MispCaseImportRequest(), new StreamPart(stream, "e.json", "application/json"), TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Parts[1].Bytes.Should().Equal(7, 8, 9);
	}

	[Fact]
	public async Task ImportCaseAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Misp.ImportCaseAsync(new MispCaseImportRequest(), new ByteArrayPart([1], "e.json"), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
