using System.Net;
using Refit;
using TheHive.Api.Data.Cortex;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class CortexTests
{
	private const string ActionJson = """
		{
			"_id":"~327696","_type":"Action","_createdBy":"emma@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1748739600000,"_updatedAt":1748739605000,"responderId":"fake-responder-id",
			"responderName":"Mailer","responderDefinition":{"name":"Mailer_1_0","version":"1.0"},"cortexId":"Cortex1",
			"cortexJobId":"fake-job-id","objectType":"Observable","objectId":"~276824","status":"Success",
			"startDate":1748739600000,"endDate":1748739605000,
			"operations":[{"tag":"ransomware","type":"AddTagToCase","status":"Success","message":"Tag added to the case."}],
			"report":"{\"success\":true}","extraData":{"k":1}
		}
		""";

	private const string MinimalActionJson = """
		{
			"_id":"~1","_type":"Action","_createdBy":"emma@example.com","_createdAt":1748739600000,"responderId":"r",
			"objectType":"Case","objectId":"~2","status":"Waiting","startDate":1748739600000,"operations":"[]","report":"{}","extraData":{}
		}
		""";

	private const string JobJson = """
		{
			"_id":"~380928","_type":"case_artifact_job","_createdBy":"emma@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1748739600000,"_updatedAt":1748739605000,"analyzerId":"fake-analyzer-id",
			"analyzerName":"VirusTotal_GetReport","analyzerDefinition":"VirusTotal_GetReport_3_0","status":"Success",
			"startDate":1748739600000,"endDate":1748739605000,"report":{"summary":{"taxonomies":[]}},"cortexId":"Cortex1",
			"cortexJobId":"fake-job-id","id":"~380928","case_artifact":{"_id":"~344112","dataType":"ip"},"operations":"[]"
		}
		""";

	private const string MinimalJobJson = """
		{
			"_id":"~1","_type":"case_artifact_job","_createdBy":"emma@example.com","_createdAt":1748739600000,
			"analyzerId":"a","analyzerName":"n","status":"Waiting","startDate":1748739600000,"cortexId":"c","cortexJobId":"j","id":"~1"
		}
		""";

	private const string AnalyzerJson = """
		{
			"id":"fake-analyzer-id","name":"VirusTotal_GetReport","version":"3.0",
			"description":"Search for a specific hash, IP, domain, or URL in the VirusTotal database.",
			"dataTypeList":["hash","domain","ip","url"],"cortexIds":["Cortex1"]
		}
		""";

	private const string TemplateJson = """{"id":"~4321","analyzerId":"VirusTotal_GetReport","content":"<div>report</div>"}""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertAction(CortexAction action)
	{
		action.Id.Should().Be("~327696");
		action.Type.Should().Be("Action");
		action.CreatedBy.Should().Be("emma@example.com");
		action.UpdatedBy.Should().Be("sami@example.com");
		action.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		action.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		action.ResponderId.Should().Be("fake-responder-id");
		action.ResponderName.Should().Be("Mailer");
		action.ResponderDefinition!.Value.GetProperty("name").GetString().Should().Be("Mailer_1_0");
		action.CortexId.Should().Be("Cortex1");
		action.CortexJobId.Should().Be("fake-job-id");
		action.ObjectType.Should().Be("Observable");
		action.ObjectId.Should().Be("~276824");
		action.Status.Should().Be(CortexActionStatus.Success);
		action.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		action.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		action.Operations!.Value[0].GetProperty("type").GetString().Should().Be("AddTagToCase");
		action.Report!.Value.GetString().Should().Be("{\"success\":true}");
		action.ExtraData["k"].GetInt32().Should().Be(1);
	}

	[Fact]
	public async Task CreateActionAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, ActionJson);
		using var client = TestClient.Create(stub);

		var action = await client.Cortex.CreateActionAsync(
			new CortexActionInput
			{
				ResponderId = "fake-responder-id",
				CortexId = "Cortex1",
				ObjectType = "case",
				ObjectId = "~276824",
				Parameters = new() { ["to"] = "sami@example.com" },
				Tlp = 2
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/action");
		stub.Calls[0].Body.Should().Be(
			"""{"responderId":"fake-responder-id","cortexId":"Cortex1","objectType":"case","objectId":"~276824","parameters":{"to":"sami@example.com"},"tlp":2}""");
		AssertAction(action);
	}

	[Fact]
	public async Task CreateActionAsync_RequiredOnly_OmitsOptionals_AndMinimalResponseMapsToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalActionJson);
		using var client = TestClient.Create(stub);

		var action = await client.Cortex.CreateActionAsync(
			new CortexActionInput { ResponderId = "r", ObjectType = "case", ObjectId = "~2" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"responderId":"r","objectType":"case","objectId":"~2"}""");
		action.Status.Should().Be(CortexActionStatus.Waiting);
		action.UpdatedBy.Should().BeNull();
		action.UpdatedAt.Should().BeNull();
		action.ResponderName.Should().BeNull();
		action.ResponderDefinition.Should().BeNull();
		action.CortexId.Should().BeNull();
		action.CortexJobId.Should().BeNull();
		action.EndDate.Should().BeNull();
		action.Operations!.Value.GetString().Should().Be("[]");
		action.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateActionsAsync_PostsArrayBody()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ActionJson}]");
		using var client = TestClient.Create(stub);

		var actions = await client.Cortex.CreateActionsAsync(
			[
				new CortexActionInput { ResponderId = "r1", ObjectType = "case", ObjectId = "~1" },
				new CortexActionInput { ResponderId = "r2", ObjectType = "alert", ObjectId = "~2", Tlp = 1 }
			],
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/actions");
		stub.Calls[0].Body.Should().Be(
			"""[{"responderId":"r1","objectType":"case","objectId":"~1"},{"responderId":"r2","objectType":"alert","objectId":"~2","tlp":1}]""");
		AssertAction(actions.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task ListAnalyzersAsync_SendsRangeQuery_AndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{AnalyzerJson}]");
		using var client = TestClient.Create(stub);

		var analyzers = await client.Cortex.ListAnalyzersAsync("all", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer");
		stub.Calls[0].Uri.Query.Should().Be("?range=all");
		stub.Calls[0].Body.Should().BeNull();
		var analyzer = analyzers.Should().ContainSingle().Subject;
		analyzer.Id.Should().Be("fake-analyzer-id");
		analyzer.Name.Should().Be("VirusTotal_GetReport");
		analyzer.Version.Should().Be("3.0");
		analyzer.Description.Should().StartWith("Search for a specific hash");
		analyzer.DataTypeList.Should().Equal("hash", "domain", "ip", "url");
		analyzer.CortexIds.Should().Equal("Cortex1");
	}

	[Fact]
	public async Task ListAnalyzersAsync_WithoutRange_OmitsTheQuery()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		var analyzers = await client.Cortex.ListAnalyzersAsync(cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
		analyzers.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAnalyzerAsync_Gets_AndAbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, """{"id":"a","name":"n","version":"1","description":"d"}""");
		using var client = TestClient.Create(stub);

		var analyzer = await client.Cortex.GetAnalyzerAsync("a b", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/a%20b");
		analyzer.DataTypeList.Should().BeEmpty();
		analyzer.CortexIds.Should().BeEmpty();
	}

	[Fact]
	public async Task ListAnalyzersByTypeAsync_GetsByDataType()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{AnalyzerJson}]");
		using var client = TestClient.Create(stub);

		var analyzers = await client.Cortex.ListAnalyzersByTypeAsync("ip", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/type/ip");
		analyzers.Should().ContainSingle().Which.Name.Should().Be("VirusTotal_GetReport");
	}

	[Fact]
	public async Task CreateAnalyzerTemplateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, TemplateJson);
		using var client = TestClient.Create(stub);

		var template = await client.Cortex.CreateAnalyzerTemplateAsync(
			new AnalyzerTemplateCreateRequest { AnalyzerId = "VirusTotal_GetReport", Content = "report content" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/template");
		stub.Calls[0].Body.Should().Be("""{"analyzerId":"VirusTotal_GetReport","content":"report content"}""");
		template.Id.Should().Be("~4321");
		template.AnalyzerId.Should().Be("VirusTotal_GetReport");
		template.Content.Should().Be("<div>report</div>");
	}

	[Fact]
	public async Task CreateAnalyzerTemplateAsync_HtmlContent_IsWrittenWithTheDefaultJsonEscapes()
	{
		var stub = Stub(HttpStatusCode.Created, TemplateJson);
		using var client = TestClient.Create(stub);

		await client.Cortex.CreateAnalyzerTemplateAsync(
			new AnalyzerTemplateCreateRequest { AnalyzerId = "A", Content = "<div class=\"x\">&</div>" },
			TestContext.Current.CancellationToken);

		// System.Text.Json escapes <, >, & and " as \u00XX by default. These are valid JSON, decoded identically by the server.
		// Changing the encoder (TheHiveJson.Options) must be a deliberate decision, so the exact body is pinned here.
		var bs = '\\';
		stub.Calls[0].Body.Should().Be(
			$$"""{"analyzerId":"A","content":"{{bs}}u003Cdiv class={{bs}}u0022x{{bs}}u0022{{bs}}u003E{{bs}}u0026{{bs}}u003C/div{{bs}}u003E"}""");
	}

	[Fact]
	public async Task ImportAnalyzerTemplatesAsync_UploadsTheArchiveAsATemplatesPart()
	{
		var stub = Stub(HttpStatusCode.OK, """{"VirusTotal_GetReport":true,"Abuse_Finder":false}""");
		using var client = TestClient.Create(stub);
		byte[] zip = [0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF];

		var result = await client.Cortex.ImportAnalyzerTemplatesAsync(
			new ByteArrayPart(zip, "report-templates.zip", "application/zip"),
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/template/_import");
		call.ContentType.Should().Be("multipart/form-data");
		var part = call.Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("templates");
		part.FileName.Should().Be("report-templates.zip");
		part.ContentType.Should().Be("application/zip");
		part.Bytes.Should().Equal(zip);
		result["VirusTotal_GetReport"].Should().BeTrue();
		result["Abuse_Finder"].Should().BeFalse();
	}

	[Fact]
	public async Task DeleteAnalyzerTemplateAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Cortex.DeleteAnalyzerTemplateAsync("~4321", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/template/~4321");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAnalyzerTemplateAsync_PatchesBody_AndOmitsAnUnsetContent()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, TemplateJson);
		stub.Enqueue(HttpStatusCode.OK, TemplateJson);
		using var client = TestClient.Create(stub);

		var template = await client.Cortex.UpdateAnalyzerTemplateAsync(
			"~4321",
			new AnalyzerTemplateUpdateRequest { Content = "new content" },
			TestContext.Current.CancellationToken);
		await client.Cortex.UpdateAnalyzerTemplateAsync("~4321", new AnalyzerTemplateUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/template/~4321");
		stub.Calls[0].Body.Should().Be("""{"content":"new content"}""");
		stub.Calls[1].Body.Should().Be("{}");
		template.Content.Should().Be("<div>report</div>");
	}

	[Fact]
	public async Task GetAnalyzerTemplateContentAsync_ReturnsTheRawTextBody()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "<div>report</div>", response => response.Content = new StringContent("<div>report</div>", System.Text.Encoding.UTF8, "text/plain"));
		using var client = TestClient.Create(stub);

		var content = await client.Cortex.GetAnalyzerTemplateContentAsync("VirusTotal_GetReport", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/analyzer/template/content/VirusTotal_GetReport");
		content.Should().Be("<div>report</div>");
	}
}
