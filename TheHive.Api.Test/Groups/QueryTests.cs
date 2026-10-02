using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Query;
using TheHive.Api.Querying;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class QueryTests
{
	// The spec's request example for POST /api/v1/query.
	private const string SpecRequestJson =
		"""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"severity","_value":3}},{"_name":"sort","_fields":[{"_createdAt":"desc"}]},{"_name":"page","from":0,"to":10,"extraData":["taskStats","shareCount","total"]}],"excludeFields":["description"]}""";

	// The spec's response example for POST /api/v1/query (two cases).
	private const string SpecResponseJson = """
		[
			{
				"_id":"~98112","_type":"Case","_createdBy":"alice@example.com","_createdAt":1745539200000,"_updatedBy":"alice@example.com",
				"_updatedAt":1745625600000,"number":412,"title":"Suspicious Ransomware Activity","severity":3,"severityLabel":"High",
				"startDate":1745539200000,"tags":["ransomware","file-encryption","TheOrganization"],"flag":true,"tlp":3,"tlpLabel":"Red",
				"pap":2,"papLabel":"Amber","status":"InProgress","stage":"InProgress","assignee":"alice@example.com",
				"access":{"_kind":"OrganisationAccessKind"},"customFields":[],
				"extraData":{"taskStats":{"Waiting":1,"InProgress":4,"Completed":4,"Cancel":0},"shareCount":2},
				"newDate":1745539200000,"inProgressDate":1745542800000,"timeToDetect":0
			},
			{
				"_id":"~98098","_type":"Case","_createdBy":"alice@example.com","_createdAt":1745452800000,"_updatedBy":"alice@example.com",
				"_updatedAt":1745625600000,"number":408,"title":"Unauthorized Access – Compromised Employee Account","severity":3,
				"severityLabel":"High","startDate":1745452800000,"endDate":1745625600000,"tags":["intrusion","compromised-account","TheOrganization"],
				"flag":true,"tlp":3,"tlpLabel":"Red","pap":3,"papLabel":"Red","status":"Closed","stage":"Closed","impactStatus":"WithImpact",
				"assignee":"alice@example.com","access":{"_kind":"OrganisationAccessKind"},"customFields":[],
				"extraData":{"taskStats":{"Waiting":0,"InProgress":0,"Completed":5,"Cancel":0},"shareCount":1},
				"newDate":1745452800000,"inProgressDate":1745456400000,"closedDate":1745625600000,"timeToDetect":0,
				"timeToResolve":172800000,"handlingDuration":172800000
			}
		]
		""";

	private static QueryBuilder SpecQuery() => QueryBuilder.ListCases()
		.Filter("severity", 3)
		.Sort("_createdAt", SortDirection.Descending)
		.Page(0, 10, "taskStats", "shareCount", "total")
		.Exclude("description");

	private static StubHandler Stub(HttpStatusCode status, string json = "", Action<HttpResponseMessage>? configure = null)
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json, configure);
		return stub;
	}

	private static void AssertSpecCases(List<Case> cases)
	{
		cases.Should().HaveCount(2);
		var first = cases[0];
		first.Id.Should().Be("~98112");
		first.Type.Should().Be("Case");
		first.CreatedBy.Should().Be("alice@example.com");
		first.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745539200000));
		first.UpdatedBy.Should().Be("alice@example.com");
		first.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745625600000));
		first.Number.Should().Be(412);
		first.Title.Should().Be("Suspicious Ransomware Activity");
		first.Severity.Should().Be(Severity.High);
		first.SeverityLabel.Should().Be("High");
		first.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745539200000));
		first.Tags.Should().Equal("ransomware", "file-encryption", "TheOrganization");
		first.Flag.Should().BeTrue();
		first.Tlp.Should().Be(3);
		first.TlpLabel.Should().Be("Red");
		first.Pap.Should().Be(Pap.Amber);
		first.PapLabel.Should().Be("Amber");
		first.Status.Should().Be("InProgress");
		first.Stage.Should().Be(CaseStage.InProgress);
		first.Assignee.Should().Be("alice@example.com");
		first.Access.Kind.Should().Be(AccessKind.OrganisationAccessKind);
		first.CustomFields.Should().BeEmpty();
		first.ExtraData["taskStats"].GetRawText().Should().Be("""{"Waiting":1,"InProgress":4,"Completed":4,"Cancel":0}""");
		first.ExtraData["shareCount"].GetInt32().Should().Be(2);
		first.NewDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745539200000));
		first.InProgressDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745542800000));
		first.TimeToDetect.Should().Be(0);
		var second = cases[1];
		second.Number.Should().Be(408);
		second.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745625600000));
		second.Stage.Should().Be(CaseStage.Closed);
		second.ImpactStatus.Should().Be(ImpactStatus.WithImpact);
		second.ClosedDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1745625600000));
		second.TimeToResolve.Should().Be(172800000);
		second.HandlingDuration.Should().Be(172800000);
	}

	[Fact]
	public async Task RunAsync_PostsTheSpecExampleAndReturnsTheRawResult()
	{
		var stub = Stub(HttpStatusCode.OK, SpecResponseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Query.RunAsync(SpecQuery().Build(), "cases", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/query");
		stub.Calls[0].Uri.Query.Should().Be("?name=cases");
		stub.Calls[0].Body.Should().Be(SpecRequestJson);
		result.ValueKind.Should().Be(JsonValueKind.Array);
		result.GetArrayLength().Should().Be(2);
	}

	[Fact]
	public async Task RunAsync_WithoutName_SendsNoQueryString()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		await client.Query.RunAsync(QueryBuilder.ListCases().Build(), cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"query":[{"_name":"listCase"}]}""");
	}

	[Fact]
	public async Task RunAsync_HandWrittenRequest_SendsTheStepsAsGiven()
	{
		var stub = Stub(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);
		var request = new QueryRequest
		{
			Query = [new() { ["_name"] = "listAuditFromObject", ["id"] = "~327925760" }],
			IncludeFields = ["action"],
			ExcludeFields = ["details"]
		};

		await client.Query.RunAsync(request, cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"query":[{"_name":"listAuditFromObject","id":"~327925760"}],"includeFields":["action"],"excludeFields":["details"]}""");
	}

	[Fact]
	public async Task RunAsync_BadRequest_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Invalid query"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Query.RunAsync(QueryBuilder.List("listNothing").Build(), cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest" && e.Message == "Invalid query");
	}

	[Fact]
	public async Task RunAsyncOfT_MapsEveryResult()
	{
		var stub = Stub(HttpStatusCode.OK, SpecResponseJson);
		using var client = TestClient.Create(stub);

		var cases = await client.Query.RunAsync<Case>(SpecQuery(), "cases", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?name=cases");
		stub.Calls[0].Body.Should().Be(SpecRequestJson);
		AssertSpecCases(cases);
	}

	[Fact]
	public async Task RunAsyncOfT_SingleObjectResult_IsAOneItemList()
	{
		var stub = Stub(HttpStatusCode.OK, """{"_id":"~1234","_type":"Case","number":7,"title":"t"}""");
		using var client = TestClient.Create(stub);

		var cases = await client.Query.RunAsync<Case>(QueryBuilder.GetCase("~1234"), cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"query":[{"_name":"getCase","idOrName":"~1234"}]}""");
		cases.Should().ContainSingle().Which.Number.Should().Be(7);
	}

	[Fact]
	public async Task RunAsyncOfT_CountQuery_Throws()
	{
		using var client = TestClient.Create(new StubHandler());

		var act = () => client.Query.RunAsync<Case>(QueryBuilder.ListCases().Count(), cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("builder").WithMessage("*RunCountAsync*");
	}

	[Fact]
	public async Task RunAsyncOfT_NullArguments_Throw()
	{
		using var client = TestClient.Create(new StubHandler());

		var nullQuery = () => QueryExtensions.RunAsync<Case>(null!, QueryBuilder.ListCases(), cancellationToken: TestContext.Current.CancellationToken);
		var nullBuilder = () => client.Query.RunAsync<Case>(null!, cancellationToken: TestContext.Current.CancellationToken);

		await nullQuery.Should().ThrowAsync<ArgumentNullException>();
		await nullBuilder.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task RunCountAsync_PostsCountStepAndReturnsTheNumber()
	{
		var stub = Stub(HttpStatusCode.OK, "42");
		using var client = TestClient.Create(stub);

		var count = await client.Query.RunCountAsync(QueryBuilder.ListCases().Filter("status", "New").Count(), "count", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/query");
		stub.Calls[0].Uri.Query.Should().Be("?name=count");
		stub.Calls[0].Body.Should().Be("""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"status","_value":"New"}},{"_name":"count"}]}""");
		count.Should().Be(42);
	}

	[Fact]
	public async Task RunCountAsync_WithoutCountStep_Throws()
	{
		using var client = TestClient.Create(new StubHandler());

		var act = () => client.Query.RunCountAsync(QueryBuilder.ListCases(), cancellationToken: TestContext.Current.CancellationToken);
		var nullQuery = () => QueryExtensions.RunCountAsync(null!, QueryBuilder.ListCases().Count(), cancellationToken: TestContext.Current.CancellationToken);
		var nullBuilder = () => client.Query.RunCountAsync(null!, cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<ArgumentException>()).WithParameterName("builder").WithMessage("*Count()*");
		await nullQuery.Should().ThrowAsync<ArgumentNullException>();
		await nullBuilder.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task RunWithResponseAsync_ExposesTheTotalHeader()
	{
		var stub = Stub(HttpStatusCode.OK, "[]", r => r.Headers.Add("X-Total", "57"));
		using var client = TestClient.Create(stub);

		using var response = await client.Query.RunWithResponseAsync(SpecQuery().Build(), "cases", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/query");
		stub.Calls[0].Uri.Query.Should().Be("?name=cases");
		stub.Calls[0].Body.Should().Be(SpecRequestJson);
		response.IsSuccessStatusCode.Should().BeTrue();
		response.Headers.GetValues("X-Total").Should().Equal("57");
		(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("[]");
	}

	[Fact]
	public async Task RunWithResponseAsync_ErrorStatus_IsReturnedNotThrown()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Invalid query"}""");
		using var client = TestClient.Create(stub);

		using var response = await client.Query.RunWithResponseAsync(QueryBuilder.ListCases().Build(), cancellationToken: TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
		stub.Calls[0].Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task RunPageAsync_MapsResultsAndTotal()
	{
		var stub = Stub(HttpStatusCode.OK, SpecResponseJson, r => r.Headers.Add("X-Total", "57"));
		using var client = TestClient.Create(stub);

		var page = await client.Query.RunPageAsync<Case>(SpecQuery(), "cases", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?name=cases");
		stub.Calls[0].Body.Should().Be(SpecRequestJson);
		AssertSpecCases(page.Items);
		page.Total.Should().Be(57);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("not-a-number")]
	public async Task RunPageAsync_MissingOrInvalidTotal_IsNull(string? header)
	{
		var stub = Stub(HttpStatusCode.OK, "[]", r =>
		{
			if (header is not null)
			{
				r.Headers.Add("X-Total", header);
			}
		});
		using var client = TestClient.Create(stub);

		var page = await client.Query.RunPageAsync<Case>(QueryBuilder.ListCases(), cancellationToken: TestContext.Current.CancellationToken);

		page.Items.Should().BeEmpty();
		page.Total.Should().BeNull();
	}

	[Fact]
	public async Task RunPageAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Query.RunPageAsync<Case>(QueryBuilder.ListCases(), cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError" && e.Message == "Not allowed");
	}

	[Fact]
	public async Task RunPageAsync_InvalidArguments_Throw()
	{
		using var client = TestClient.Create(new StubHandler());

		var count = () => client.Query.RunPageAsync<Case>(QueryBuilder.ListCases().Count(), cancellationToken: TestContext.Current.CancellationToken);
		var nullQuery = () => QueryExtensions.RunPageAsync<Case>(null!, QueryBuilder.ListCases(), cancellationToken: TestContext.Current.CancellationToken);

		await count.Should().ThrowAsync<ArgumentException>();
		await nullQuery.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task ExportAsync_SendsQueryAndOptionsAndReturnsTheFile()
	{
		var stub = new StubHandler();
		var csv = "number,title\r\n412,Suspicious Ransomware Activity\r\n"u8.ToArray();
		stub.EnqueueFile(csv, "text/csv", "2026-10-02_export.csv");
		using var client = TestClient.Create(stub);

		using var content = await client.Query.ExportAsync(
			"""[{"_name":"listCase"}]""",
			"""{"format":"csv","model":"Case"}""",
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/export");
		stub.Calls[0].Uri.Query.Should().Be(
			"?query=%5B%7B%22_name%22%3A%22listCase%22%7D%5D&options=%7B%22format%22%3A%22csv%22%2C%22model%22%3A%22Case%22%7D");
		content.Headers.ContentType!.MediaType.Should().Be("text/csv");
		content.Headers.ContentDisposition!.FileName.Should().Be("2026-10-02_export.csv");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(csv);
	}

	[Fact]
	public async Task ExportAsync_FromBuilder_WritesTheOperationsAndEveryOption()
	{
		var stub = new StubHandler();
		stub.EnqueueFile("[]"u8.ToArray(), "application/json", "cases.json");
		using var client = TestClient.Create(stub);

		using var content = await client.Query.ExportAsync(
			QueryBuilder.ListCases().Filter("severity", 3).Exclude("description"),
			new ExportOptions
			{
				Format = ExportFormat.Csv,
				FileName = "cases",
				Model = ExportModel.Case,
				ProtectData = false,
				Fields = ["number", "title", "customFields.threat-type"],
				Delimiter = ';',
				QuoteChar = '|',
				EscapeChar = '\\'
			},
			TestContext.Current.CancellationToken);

		var query = System.Web.HttpUtility.ParseQueryString(stub.Calls[0].Uri.Query);
		query["query"].Should().Be("""[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"severity","_value":3}}]""");
		query["options"].Should().Be(
			"""{"format":"csv","fileName":"cases","model":"Case","protectData":false,"fields":["number","title","customFields.threat-type"],"delimiter":";","quoteChar":"|","escapeChar":"\\"}""");
		(await content.ReadAsStringAsync(TestContext.Current.CancellationToken)).Should().Be("[]");
	}

	[Theory]
	[InlineData(ExportFormat.Json, ExportModel.Alert, """{"format":"json","model":"Alert"}""")]
	[InlineData(ExportFormat.Txt, ExportModel.Observable, """{"format":"txt","model":"Observable"}""")]
	[InlineData(ExportFormat.Misp, ExportModel.Observable, """{"format":"misp","model":"Observable"}""")]
	[InlineData(ExportFormat.Csv, ExportModel.User, """{"format":"csv","model":"User"}""")]
	[InlineData(ExportFormat.Csv, ExportModel.Organisation, """{"format":"csv","model":"Organisation"}""")]
	[InlineData(ExportFormat.Csv, ExportModel.Procedure, """{"format":"csv","model":"Procedure"}""")]
	[InlineData(ExportFormat.Csv, ExportModel.Task, """{"format":"csv","model":"Task"}""")]
	public void ExportOptions_WriteTheDocumentedWireNames(ExportFormat format, ExportModel model, string expected)
	{
		JsonSerializer.Serialize(new ExportOptions { Format = format, Model = model }, TheHiveJson.Options).Should().Be(expected);
	}

	[Fact]
	public async Task ExportAsync_FromBuilder_NullArguments_Throw()
	{
		using var client = TestClient.Create(new StubHandler());
		var options = new ExportOptions { Format = ExportFormat.Csv, Model = ExportModel.Case };

		var nullQuery = () => QueryExtensions.ExportAsync(null!, QueryBuilder.ListCases(), options, TestContext.Current.CancellationToken);
		var nullBuilder = () => client.Query.ExportAsync((QueryBuilder)null!, options, TestContext.Current.CancellationToken);
		var nullOptions = () => client.Query.ExportAsync(QueryBuilder.ListCases(), null!, TestContext.Current.CancellationToken);

		await nullQuery.Should().ThrowAsync<ArgumentNullException>();
		await nullBuilder.Should().ThrowAsync<ArgumentNullException>();
		await nullOptions.Should().ThrowAsync<ArgumentNullException>();
	}

	[Fact]
	public async Task ExportAsync_BadRequest_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"model is mandatory"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Query.ExportAsync("[]", "{}", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task GetExportFieldsAsync_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, """
			{"fieldsByModel":{
				"Case":[
					{"fieldPath":"number","fieldType":"number","selectedByDefault":true},
					{"fieldPath":"tags","fieldType":"string[]","selectedByDefault":true},
					{"fieldPath":"endDate","fieldType":"date","selectedByDefault":false}
				],
				"Observable":[{"fieldPath":"attachment.name","fieldType":"string","selectedByDefault":false}]
			}}
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Query.GetExportFieldsAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/export/_fields");
		result.FieldsByModel.Keys.Should().Equal("Case", "Observable");
		result.FieldsByModel["Case"].Select(f => f.FieldPath).Should().Equal("number", "tags", "endDate");
		result.FieldsByModel["Case"].Select(f => f.FieldType).Should().Equal("number", "string[]", "date");
		result.FieldsByModel["Case"].Select(f => f.SelectedByDefault).Should().Equal(true, true, false);
		var observable = result.FieldsByModel["Observable"].Should().ContainSingle().Subject;
		observable.FieldPath.Should().Be("attachment.name");
		observable.FieldType.Should().Be("string");
		observable.SelectedByDefault.Should().BeFalse();
	}
}
