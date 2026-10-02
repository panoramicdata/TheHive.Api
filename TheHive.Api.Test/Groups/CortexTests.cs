using System.Net;
using Refit;
using TheHive.Api.Data.Cortex;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CortexTests
{
	private const string ActionJson = """
		{
			"_id":"~327696","_type":"Action","_createdBy":"emma@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1748739600000,"_updatedAt":1748739605000,"responderId":"b7fa3af4dd7a6de1d8c1f1f4a9c6e7ab",
			"responderName":"Mailer","responderDefinition":{"name":"Mailer_1_0","version":"1.0"},"cortexId":"Cortex1",
			"cortexJobId":"AWmX6HGxi3s3zwsBQJer","objectType":"Observable","objectId":"~276824","status":"Success",
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
			"_createdAt":1748739600000,"_updatedAt":1748739605000,"analyzerId":"220483fde9608c580fb6a2508ff3d2d3",
			"analyzerName":"VirusTotal_GetReport","analyzerDefinition":"VirusTotal_GetReport_3_0","status":"Success",
			"startDate":1748739600000,"endDate":1748739605000,"report":{"summary":{"taxonomies":[]}},"cortexId":"Cortex1",
			"cortexJobId":"AWmX6HGxi3s3zwsBQJer","id":"~380928","case_artifact":{"_id":"~344112","dataType":"ip"},"operations":"[]"
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
			"id":"220483fde9608c580fb6a2508ff3d2d3","name":"VirusTotal_GetReport","version":"3.0",
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
		action.ResponderId.Should().Be("b7fa3af4dd7a6de1d8c1f1f4a9c6e7ab");
		action.ResponderName.Should().Be("Mailer");
		action.ResponderDefinition!.Value.GetProperty("name").GetString().Should().Be("Mailer_1_0");
		action.CortexId.Should().Be("Cortex1");
		action.CortexJobId.Should().Be("AWmX6HGxi3s3zwsBQJer");
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
				ResponderId = "b7fa3af4dd7a6de1d8c1f1f4a9c6e7ab",
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
			"""{"responderId":"b7fa3af4dd7a6de1d8c1f1f4a9c6e7ab","cortexId":"Cortex1","objectType":"case","objectId":"~276824","parameters":{"to":"sami@example.com"},"tlp":2}""");
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
		analyzer.Id.Should().Be("220483fde9608c580fb6a2508ff3d2d3");
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

	[Fact]
	public async Task CreateJobAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, JobJson);
		using var client = TestClient.Create(stub);

		var job = await client.Cortex.CreateJobAsync(
			new CortexJobCreateRequest
			{
				AnalyzerId = "220483fde9608c580fb6a2508ff3d2d3",
				CortexId = "Cortex1",
				ArtifactId = "~344112",
				Parameters = new() { ["comment"] = "Investigating phishing campaign" }
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/job");
		stub.Calls[0].Body.Should().Be(
			"""{"analyzerId":"220483fde9608c580fb6a2508ff3d2d3","cortexId":"Cortex1","artifactId":"~344112","parameters":{"comment":"Investigating phishing campaign"}}""");
		job.Id.Should().Be("~380928");
		job.Type.Should().Be("case_artifact_job");
		job.CreatedBy.Should().Be("emma@example.com");
		job.UpdatedBy.Should().Be("sami@example.com");
		job.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		job.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		job.AnalyzerId.Should().Be("220483fde9608c580fb6a2508ff3d2d3");
		job.AnalyzerName.Should().Be("VirusTotal_GetReport");
		job.AnalyzerDefinition!.Value.GetString().Should().Be("VirusTotal_GetReport_3_0");
		job.Status.Should().Be("Success");
		job.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		job.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		job.Report!.Value.GetProperty("summary").GetProperty("taxonomies").GetArrayLength().Should().Be(0);
		job.CortexId.Should().Be("Cortex1");
		job.CortexJobId.Should().Be("AWmX6HGxi3s3zwsBQJer");
		job.JobId.Should().Be("~380928");
		job.CaseArtifact!.Value.GetProperty("dataType").GetString().Should().Be("ip");
		job.Operations!.Value.GetString().Should().Be("[]");
	}

	[Fact]
	public async Task CreateJobAsync_RequiredOnly_OmitsParameters()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalJobJson);
		using var client = TestClient.Create(stub);

		var job = await client.Cortex.CreateJobAsync(
			new CortexJobCreateRequest { AnalyzerId = "a", CortexId = "c", ArtifactId = "~1" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"analyzerId":"a","cortexId":"c","artifactId":"~1"}""");
		job.UpdatedBy.Should().BeNull();
		job.UpdatedAt.Should().BeNull();
		job.AnalyzerDefinition.Should().BeNull();
		job.EndDate.Should().BeNull();
		job.Report.Should().BeNull();
		job.CaseArtifact.Should().BeNull();
		job.Operations.Should().BeNull();
	}

	[Fact]
	public async Task GetJobAsync_Gets()
	{
		var stub = Stub(HttpStatusCode.OK, JobJson);
		using var client = TestClient.Create(stub);

		var job = await client.Cortex.GetJobAsync("~380928", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/job/~380928");
		job.Id.Should().Be("~380928");
	}

	[Fact]
	public async Task ListRespondersAsync_GetsForEntity_AndMapsEveryField()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			"""[{"id":"b7fa","name":"Mailer","version":"1.0","description":"Send an email.","dataTypeList":["thehive:case"],"cortexIds":["Cortex1"]}]""");
		using var client = TestClient.Create(stub);

		var responders = await client.Cortex.ListRespondersAsync("case_artifact", "~276824", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/responder/case_artifact/~276824");
		var responder = responders.Should().ContainSingle().Subject;
		responder.Id.Should().Be("b7fa");
		responder.Name.Should().Be("Mailer");
		responder.Version.Should().Be("1.0");
		responder.Description.Should().Be("Send an email.");
		responder.DataTypeList.Should().Equal("thehive:case");
		responder.CortexIds.Should().Equal("Cortex1");
	}

	[Fact]
	public async Task ListRespondersAsync_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, """[{"id":"r","name":"n","version":"1","description":"d"}]""");
		using var client = TestClient.Create(stub);

		var responders = await client.Cortex.ListRespondersAsync("case", "~1", TestContext.Current.CancellationToken);

		responders[0].DataTypeList.Should().BeEmpty();
		responders[0].CortexIds.Should().BeEmpty();
	}

	[Fact]
	public async Task ListRespondersForEntityTypeAsync_GetsByEntityType()
	{
		var stub = Stub(HttpStatusCode.OK, """[{"id":"b7fa","name":"Mailer","description":"Send an email through an SMTP relay."}]""");
		using var client = TestClient.Create(stub);

		var responders = await client.Cortex.ListRespondersForEntityTypeAsync("case_task_log", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/responders/case_task_log");
		var responder = responders.Should().ContainSingle().Subject;
		responder.Id.Should().Be("b7fa");
		responder.Name.Should().Be("Mailer");
		responder.Description.Should().Be("Send an email through an SMTP relay.");
	}

	[Fact]
	public async Task ListActionsAsync_SendsFilterSortAndPaging_AsEncodedQueryValues()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ActionJson}]");
		using var client = TestClient.Create(stub);

		var actions = await client.Cortex.ListActionsAsync(
			"case",
			"~276824",
			"""{"_eq":{"_field":"status","_value":"Success"}}""",
			"""[{"field":"responderId","direction":"asc"}]""",
			0,
			30,
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/responder-execution/case/~276824");
		stub.Calls[0].Uri.Query.Should().Be(
			"?filter=%7B%22_eq%22%3A%7B%22_field%22%3A%22status%22%2C%22_value%22%3A%22Success%22%7D%7D&sort=%5B%7B%22field%22%3A%22responderId%22%2C%22direction%22%3A%22asc%22%7D%5D&pageFrom=0&pageTo=30");
		AssertAction(actions.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task ListActionsAsync_WithoutOptionals_SendsNoQuery()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		var actions = await client.Cortex.ListActionsAsync("alert", "~9", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/responder-execution/alert/~9");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		actions.Should().BeEmpty();
	}

	[Fact]
	public async Task CountActionsAsync_SendsFilter_AndReturnsTheCount()
	{
		var stub = Stub(HttpStatusCode.OK, "4");
		using var client = TestClient.Create(stub);

		var count = await client.Cortex.CountActionsAsync("case", "~276824", """{"_eq":{"_field":"status","_value":"Success"}}""", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/responder-execution/case/~276824/count");
		stub.Calls[0].Uri.Query.Should().Be("?filter=%7B%22_eq%22%3A%7B%22_field%22%3A%22status%22%2C%22_value%22%3A%22Success%22%7D%7D");
		count.Should().Be(4);
	}

	[Fact]
	public async Task CountActionsAsync_WithoutFilter_SendsNoQuery()
	{
		var stub = Stub(HttpStatusCode.OK, "0");
		using var client = TestClient.Create(stub);

		var count = await client.Cortex.CountActionsAsync("alert", "~9", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
		count.Should().Be(0);
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var action = new CortexAction();
		action.Id.Should().BeEmpty();
		action.Type.Should().BeEmpty();
		action.CreatedBy.Should().BeEmpty();
		action.ResponderId.Should().BeEmpty();
		action.ObjectType.Should().BeEmpty();
		action.ObjectId.Should().BeEmpty();
		action.Status.Should().Be(CortexActionStatus.Unknown);
		action.ExtraData.Should().BeEmpty();

		var job = new CortexJob();
		job.Id.Should().BeEmpty();
		job.Type.Should().BeEmpty();
		job.CreatedBy.Should().BeEmpty();
		job.AnalyzerId.Should().BeEmpty();
		job.AnalyzerName.Should().BeEmpty();
		job.Status.Should().BeEmpty();
		job.CortexId.Should().BeEmpty();
		job.CortexJobId.Should().BeEmpty();
		job.JobId.Should().BeEmpty();

		var analyzer = new CortexAnalyzer();
		analyzer.Id.Should().BeEmpty();
		analyzer.Name.Should().BeEmpty();
		analyzer.Version.Should().BeEmpty();
		analyzer.Description.Should().BeEmpty();
		analyzer.DataTypeList.Should().BeEmpty();
		analyzer.CortexIds.Should().BeEmpty();

		var responder = new CortexResponder();
		responder.Id.Should().BeEmpty();
		responder.Name.Should().BeEmpty();
		responder.Description.Should().BeEmpty();

		var entityResponder = new CortexEntityResponder();
		entityResponder.Id.Should().BeEmpty();
		entityResponder.Name.Should().BeEmpty();
		entityResponder.Version.Should().BeEmpty();
		entityResponder.Description.Should().BeEmpty();
		entityResponder.DataTypeList.Should().BeEmpty();
		entityResponder.CortexIds.Should().BeEmpty();

		var template = new AnalyzerTemplate();
		template.Id.Should().BeEmpty();
		template.AnalyzerId.Should().BeEmpty();
		template.Content.Should().BeEmpty();
	}

	[Fact]
	public async Task UnknownActionStatus_MapsToUnknown()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalActionJson.Replace("Waiting", "Brand-new-state", StringComparison.Ordinal));
		using var client = TestClient.Create(stub);

		var action = await client.Cortex.CreateActionAsync(
			new CortexActionInput { ResponderId = "r", ObjectType = "case", ObjectId = "~2" },
			TestContext.Current.CancellationToken);

		action.Status.Should().Be(CortexActionStatus.Unknown);
	}

	[Fact]
	public async Task GetJobAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Job not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Cortex.GetJobAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
