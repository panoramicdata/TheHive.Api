using System.Net;
using Refit;
using TheHive.Api.Data.Cortex;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class CortexTests
{
	[Fact]
	public async Task CreateJobAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, JobJson);
		using var client = TestClient.Create(stub);

		var job = await client.Cortex.CreateJobAsync(
			new CortexJobCreateRequest
			{
				AnalyzerId = "fake-analyzer-id",
				CortexId = "Cortex1",
				ArtifactId = "~344112",
				Parameters = new() { ["comment"] = "Investigating phishing campaign" }
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/cortex/job");
		stub.Calls[0].Body.Should().Be(
			"""{"analyzerId":"fake-analyzer-id","cortexId":"Cortex1","artifactId":"~344112","parameters":{"comment":"Investigating phishing campaign"}}""");
		job.Id.Should().Be("~380928");
		job.Type.Should().Be("case_artifact_job");
		job.CreatedBy.Should().Be("emma@example.com");
		job.UpdatedBy.Should().Be("sami@example.com");
		job.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		job.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		job.AnalyzerId.Should().Be("fake-analyzer-id");
		job.AnalyzerName.Should().Be("VirusTotal_GetReport");
		job.AnalyzerDefinition!.Value.GetString().Should().Be("VirusTotal_GetReport_3_0");
		job.Status.Should().Be("Success");
		job.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		job.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		job.Report!.Value.GetProperty("summary").GetProperty("taxonomies").GetArrayLength().Should().Be(0);
		job.CortexId.Should().Be("Cortex1");
		job.CortexJobId.Should().Be("fake-job-id");
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
			new CortexActionsQuery
			{
				Filter = """{"_eq":{"_field":"status","_value":"Success"}}""",
				Sort = """[{"field":"responderId","direction":"asc"}]""",
				PageFrom = 0,
				PageTo = 30
			},
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
	public async Task ListActionsAsync_EmptyQueryObject_SendsNoQuery()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		await client.Cortex.ListActionsAsync("alert", "~9", new CortexActionsQuery(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task ListActionsAsync_PartialQueryObject_SendsOnlyTheSetValues()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		await client.Cortex.ListActionsAsync("case", "7", new CortexActionsQuery { PageTo = 300 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?pageTo=300");
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
