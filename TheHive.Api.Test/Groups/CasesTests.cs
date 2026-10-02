using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CasesTests
{
	private const string FullCaseJson = """
		{
			"_id":"~123","_type":"Case","_createdBy":"alice@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1700000000000,"_updatedAt":1700000001000,"number":7,"title":"Phish","description":"d",
			"severity":3,"severityLabel":"HIGH","startDate":1700000002000,"endDate":1700000003000,
			"tags":["a","b"],"flag":true,"tlp":2,"tlpLabel":"AMBER","pap":1,"papLabel":"GREEN",
			"status":"TruePositive","stage":"Closed","summary":"s","impactStatus":"WithImpact","assignee":"bob@example.com",
			"access":{"_kind":"UserAccessKind","users":["bob@example.com"]},
			"customFields":[{"_id":"~9","name":"threat-type","type":"string","value":"Malware","order":0}],
			"userPermissions":["manageCase/update"],
			"extraData":{"score":5},
			"newDate":1700000004000,"inProgressDate":1700000005000,"closedDate":1700000006000,
			"alertDate":1700000007000,"alertNewDate":1700000008000,"alertInProgressDate":1700000009000,
			"alertImportedDate":1700000010000,
			"timeToDetect":11,"timeToTriage":12,"timeToQualify":13,"timeToAcknowledge":14,"timeToResolve":15,
			"handlingDuration":16
		}
		""";

	private const string MinimalCaseJson = """
		{
			"_id":"~123","_type":"Case","_createdBy":"alice@example.com","_createdAt":1700000000000,"number":7,
			"title":"Phish","description":"d","severity":2,"severityLabel":"MEDIUM","startDate":1700000000000,
			"flag":false,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","status":"Archived2099",
			"stage":"Archived2099","access":{"_kind":"OrganisationAccessKind"},"extraData":{},
			"newDate":1700000000000,"timeToDetect":0
		}
		""";

	[Fact]
	public async Task GetAsync_MapsEveryCaseField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, FullCaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetAsync("~123", TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
		result.Id.Should().Be("~123");
		result.Type.Should().Be("Case");
		result.CreatedBy.Should().Be("alice@example.com");
		result.UpdatedBy.Should().Be("sami@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000000000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000001000));
		result.Number.Should().Be(7);
		result.Title.Should().Be("Phish");
		result.Description.Should().Be("d");
		result.Severity.Should().Be(Severity.High);
		result.SeverityLabel.Should().Be("HIGH");
		result.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000002000));
		result.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000003000));
		result.Tags.Should().Equal("a", "b");
		result.Flag.Should().BeTrue();
		result.Tlp.Should().Be(Tlp.Amber);
		result.TlpLabel.Should().Be("AMBER");
		result.Pap.Should().Be(Pap.Green);
		result.PapLabel.Should().Be("GREEN");
		result.Status.Should().Be("TruePositive");
		result.Stage.Should().Be(CaseStage.Closed);
		result.Summary.Should().Be("s");
		result.ImpactStatus.Should().Be(ImpactStatus.WithImpact);
		result.Assignee.Should().Be("bob@example.com");
		result.Access.Kind.Should().Be(AccessKind.UserAccessKind);
		result.Access.Users.Should().Equal("bob@example.com");
		var field = result.CustomFields.Should().ContainSingle().Subject;
		field.Id.Should().Be("~9");
		field.Name.Should().Be("threat-type");
		field.Type.Should().Be("string");
		field.Value.GetString().Should().Be("Malware");
		field.Order.Should().Be(0);
		result.UserPermissions.Should().Equal("manageCase/update");
		result.ExtraData["score"].GetInt32().Should().Be(5);
		result.NewDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000004000));
		result.InProgressDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000005000));
		result.ClosedDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000006000));
		result.AlertDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000007000));
		result.AlertNewDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000008000));
		result.AlertInProgressDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000009000));
		result.AlertImportedDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000010000));
		result.TimeToDetect.Should().Be(11);
		result.TimeToTriage.Should().Be(12);
		result.TimeToQualify.Should().Be(13);
		result.TimeToAcknowledge.Should().Be(14);
		result.TimeToResolve.Should().Be(15);
		result.HandlingDuration.Should().Be(16);
	}

	[Fact]
	public async Task GetAsync_UnknownStage_And_AbsentOptionals()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, MinimalCaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetAsync("7", TestContext.Current.CancellationToken);

		result.Status.Should().Be("Archived2099");
		result.Stage.Should().Be(CaseStage.Unknown);
		result.EndDate.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.ImpactStatus.Should().BeNull();
		result.Assignee.Should().BeNull();
		result.Summary.Should().BeNull();
		result.Tags.Should().BeEmpty();
		result.CustomFields.Should().BeEmpty();
		result.UserPermissions.Should().BeEmpty();
		result.Access.Kind.Should().Be(AccessKind.OrganisationAccessKind);
		result.Access.Users.Should().BeNull();
		result.TimeToResolve.Should().BeNull();
	}

	[Fact]
	public void Case_Defaults_AreEmptyNotNull()
	{
		var item = new Case();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.SeverityLabel.Should().BeEmpty();
		item.TlpLabel.Should().BeEmpty();
		item.PapLabel.Should().BeEmpty();
		item.Status.Should().BeEmpty();
		item.Access.Should().NotBeNull();
		item.ExtraData.Should().BeEmpty();
		new CustomFieldValue().Name.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsResult()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, FullCaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.CreateAsync(
			new CaseCreateRequest { Title = "Phish", Description = "d", Severity = Severity.High },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case");
		stub.Calls[0].Body.Should().Be("""{"title":"Phish","description":"d","severity":3}""");
		result.Number.Should().Be(7);
	}

	[Fact]
	public async Task CreateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, FullCaseJson);
		using var client = TestClient.Create(stub);
		var request = new CaseCreateRequest
		{
			Title = "Phish",
			Description = "d",
			Severity = Severity.Critical,
			StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1748736000000),
			EndDate = DateTimeOffset.FromUnixTimeMilliseconds(1748995200000),
			Tags = ["x"],
			Flag = true,
			Tlp = Tlp.Red,
			Pap = Pap.Red,
			Status = "New",
			Summary = "sum",
			Assignee = "lucas@example.com",
			Access = new Access { Kind = AccessKind.ExternalAccessKind, Users = ["ext@example.com"] },
			CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware", Order = 0 }],
			CaseTemplate = "Ransomware",
			Tasks =
			[
				new CaseTaskCreateRequest
				{
					Title = "Isolate",
					Group = "Containment",
					Description = "td",
					Status = CaseTaskStatus.Waiting,
					Flag = false,
					StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
					EndDate = DateTimeOffset.FromUnixTimeMilliseconds(2),
					Order = 1,
					DueDate = DateTimeOffset.FromUnixTimeMilliseconds(3),
					Assignee = "lucas@example.com",
					Mandatory = true
				}
			],
			Pages = [new PageCreateRequest { Title = "Notes", Content = "c", Order = 0, Category = "Investigation" }],
			SharingParameters =
			[
				new ShareCreateRequest
				{
					Organisation = "Org",
					Share = true,
					Profile = "analyst",
					TaskRule = SharingRule.AutoShare,
					ObservableRule = SharingRule.Manual
				}
			],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.AutoShare
		};

		await client.Cases.CreateAsync(request, TestContext.Current.CancellationToken);

		using var body = JsonDocument.Parse(stub.Calls[0].Body!);
		var root = body.RootElement;
		root.GetProperty("title").GetString().Should().Be("Phish");
		root.GetProperty("description").GetString().Should().Be("d");
		root.GetProperty("severity").GetInt32().Should().Be(4);
		root.GetProperty("startDate").GetInt64().Should().Be(1748736000000);
		root.GetProperty("endDate").GetInt64().Should().Be(1748995200000);
		root.GetProperty("tags")[0].GetString().Should().Be("x");
		root.GetProperty("flag").GetBoolean().Should().BeTrue();
		root.GetProperty("tlp").GetInt32().Should().Be(4);
		root.GetProperty("pap").GetInt32().Should().Be(3);
		root.GetProperty("status").GetString().Should().Be("New");
		root.GetProperty("summary").GetString().Should().Be("sum");
		root.GetProperty("assignee").GetString().Should().Be("lucas@example.com");
		root.GetProperty("access").GetRawText().Should().Be("""{"_kind":"ExternalAccessKind","users":["ext@example.com"]}""");
		root.GetProperty("customFields").GetRawText().Should().Be("""[{"name":"threat-type","value":"Malware","order":0}]""");
		root.GetProperty("caseTemplate").GetString().Should().Be("Ransomware");
		root.GetProperty("tasks").GetRawText().Should().Be(
			"""[{"title":"Isolate","group":"Containment","description":"td","status":"Waiting","flag":false,"startDate":1,"endDate":2,"order":1,"dueDate":3,"assignee":"lucas@example.com","mandatory":true}]""");
		root.GetProperty("pages").GetRawText().Should().Be(
			"""[{"title":"Notes","content":"c","order":0,"category":"Investigation"}]""");
		root.GetProperty("sharingParameters").GetRawText().Should().Be(
			"""[{"organisation":"Org","share":true,"profile":"analyst","taskRule":"autoShare","observableRule":"manual"}]""");
		root.GetProperty("taskRule").GetString().Should().Be("manual");
		root.GetProperty("observableRule").GetString().Should().Be("autoShare");
	}

	[Fact]
	public async Task UpdateAsync_PatchesOnlySetFields()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.UpdateAsync("~123", new CaseUpdateRequest { Title = "New" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
		stub.Calls[0].Body.Should().Be("{\"title\":\"New\"}");
	}

	[Fact]
	public async Task UpdateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new CaseUpdateRequest
		{
			Title = "t",
			Description = "d",
			Severity = Severity.Low,
			StartDate = DateTimeOffset.FromUnixTimeMilliseconds(10),
			EndDate = DateTimeOffset.FromUnixTimeMilliseconds(20),
			Tags = ["a"],
			Flag = false,
			Tlp = Tlp.Clear,
			Pap = Pap.Amber,
			Status = "InProgress",
			Summary = "s",
			Assignee = "sami@example.com",
			ImpactStatus = ImpactStatus.NotApplicable,
			CustomFields = [new CustomFieldInput { Name = "severity", Value = 3 }],
			TaskRule = SharingRule.AutoShare,
			ObservableRule = SharingRule.Manual,
			AddTags = ["ransomware"],
			RemoveTags = ["old"]
		};

		await client.Cases.UpdateAsync("~123", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			"""{"title":"t","description":"d","severity":1,"startDate":10,"endDate":20,"tags":["a"],"flag":false,"tlp":0,"pap":2,"status":"InProgress","summary":"s","assignee":"sami@example.com","impactStatus":"NotApplicable","customFields":[{"name":"severity","value":3}],"taskRule":"autoShare","observableRule":"manual","addTags":["ransomware"],"removeTags":["old"]}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteAsync("~123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task MergeAsync_PostsToMergePathAndMapsNewCase()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, FullCaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.MergeAsync("~1,~2", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		Uri.UnescapeDataString(stub.Calls[0].Uri.AbsolutePath).Should().Be("/api/v1/case/_merge/~1,~2");
		result.Id.Should().Be("~123");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Case not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Cases.GetAsync("~missing", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e =>
				e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError" && e.Message == "Case not found");
	}

	[Fact]
	public async Task GetAsync_PathPrefixedBaseUrl_KeepsPrefix()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, MinimalCaseJson);
		using var client = TestClient.Create(stub, o => o.BaseUrl = "https://hive.test/thehive");

		await client.Cases.GetAsync("~123", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.ToString().Should().Be("https://hive.test/thehive/api/v1/case/~123");
	}
}
