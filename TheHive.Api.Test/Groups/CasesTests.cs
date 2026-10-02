using Refit;
using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Procedures;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Data.Timeline;
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
		item.Access.Kind.Should().Be(AccessKind.Unknown);
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void CustomFieldValue_Defaults_AreEmptyNotNull()
	{
		var value = new CustomFieldValue();

		value.Id.Should().BeEmpty();
		value.Name.Should().BeEmpty();
		value.Type.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_AccessWithoutKind_ReadsUnknown()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, MinimalCaseJson.Replace("""{"_kind":"OrganisationAccessKind"}""", "{}"));
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetAsync("~123", TestContext.Current.CancellationToken);

		result.Access.Kind.Should().Be(AccessKind.Unknown);
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
				new ShareSettings
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

	[Theory]
	[InlineData("endDate")]
	[InlineData("summary")]
	[InlineData("assignee")]
	[InlineData("impactStatus")]
	public async Task UpdateAsync_ExplicitNull_SendsNullToUnset(string field)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = field switch
		{
			"endDate" => new CaseUpdateRequest { EndDate = null },
			"summary" => new CaseUpdateRequest { Summary = null },
			"assignee" => new CaseUpdateRequest { Assignee = null },
			_ => new CaseUpdateRequest { ImpactStatus = null }
		};

		await client.Cases.UpdateAsync("~123", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"{{field}}":null}""");
	}

	[Fact]
	public void CaseUpdateRequest_ClearableFields_DefaultToUnset()
	{
		var request = new CaseUpdateRequest();

		request.EndDate.HasValue.Should().BeFalse();
		request.Summary.HasValue.Should().BeFalse();
		request.Assignee.HasValue.Should().BeFalse();
		request.ImpactStatus.HasValue.Should().BeFalse();
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
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/_merge/~1%2C~2");
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

	[Theory]
	[InlineData("https://hive.test/thehive/", "~123", "~123")]
	[InlineData("https://hive.test/thehive", "my case", "my%20case")]
	[InlineData("https://hive.test/thehive/", "my case", "my%20case")]
	[InlineData("https://hive.test/thehive", "a/b", "a%2Fb")]
	[InlineData("https://hive.test/thehive/", "a/b", "a%2Fb")]
	[InlineData("https://hive.test/thehive", "a#b", "a%23b")]
	[InlineData("https://hive.test/thehive/", "a#b", "a%23b")]
	[InlineData("https://hive.test/thehive", "a?b", "a%3Fb")]
	[InlineData("https://hive.test/thehive/", "a?b", "a%3Fb")]
	public async Task GetAsync_EscapesIdOrNameAsOneSegment(string baseUrl, string idOrName, string expectedSegment)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, MinimalCaseJson);
		using var client = TestClient.Create(stub, o => o.BaseUrl = baseUrl);

		await client.Cases.GetAsync(idOrName, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsoluteUri.Should().Be($"https://hive.test/thehive/api/v1/case/{expectedSegment}");
	}

	private const string AttachmentJson = """
		{
			"_id":"~456789012","_type":"Attachment","_createdBy":"lucas@example.com","_updatedBy":"alice@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"name":"encrypt.ps1",
			"hashes":["fake-hash-0001","fake-hash-0002"],"size":2048,
			"contentType":"application/x-powershell","id":"fake-storage-id",
			"path":"attachments/fake-storage-id","extraData":{"links":1},"external":true
		}
		""";

	private const string ObservableJson = $$$"""
		{
			"_id":"~8529344","_type":"Observable","_createdBy":"lucas@example.com","_updatedBy":"alice@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"dataType":"ip","data":"00.01.002.003",
			"startDate":1748739600000,"attachment":{{{AttachmentJson}}},"tlp":2,"tlpLabel":"AMBER","pap":3,"papLabel":"RED",
			"tags":["Source IP"],"ioc":true,"sighted":true,"sightedAt":1748822400000,
			"reports":{"VirusTotal_GetReport":{"status":"Success"}},
			"message":"Source IP of the ransomware C2 server","extraData":{"seen":2},"ignoreSimilarity":true,"external":true
		}
		""";

	private const string MinimalObservableJson = """
		{
			"_id":"~1","_type":"Observable","_createdBy":"lucas@example.com","_createdAt":1748739600000,"dataType":"file",
			"startDate":1748739600000,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","ioc":false,"sighted":false,
			"reports":{},"extraData":{},"ignoreSimilarity":false,"external":false
		}
		""";

	[Fact]
	public async Task BulkUpdateAsync_PatchesIdsAndFields()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new CaseBulkUpdateRequest
		{
			Ids = ["~128458762", "~216513541"],
			Title = "t",
			Description = "d",
			Severity = Severity.High,
			StartDate = DateTimeOffset.FromUnixTimeMilliseconds(10),
			EndDate = null,
			Tags = ["a"],
			Flag = false,
			Tlp = Tlp.Amber,
			Pap = Pap.Amber,
			Status = "InProgress",
			Summary = "s",
			Assignee = null,
			ImpactStatus = ImpactStatus.NotApplicable,
			CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware" }],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.Manual,
			AddTags = ["ransomware"],
			RemoveTags = ["file-encryption"]
		};

		await client.Cases.BulkUpdateAsync(request, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/_bulk");
		stub.Calls[0].Body.Should().Be(
			"""{"ids":["~128458762","~216513541"],"title":"t","description":"d","severity":3,"startDate":10,"endDate":null,"tags":["a"],"flag":false,"tlp":2,"pap":2,"status":"InProgress","summary":"s","assignee":null,"impactStatus":"NotApplicable","customFields":[{"name":"threat-type","value":"Malware"}],"taskRule":"manual","observableRule":"manual","addTags":["ransomware"],"removeTags":["file-encryption"]}""");
	}

	[Fact]
	public async Task BulkUpdateAsync_SendsOnlyIdsAndSetFields()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.BulkUpdateAsync(new CaseBulkUpdateRequest { Ids = ["~1"], Flag = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"ids":["~1"],"flag":true}""");
	}

	[Fact]
	public async Task SetAccessAsync_PostsAccess()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.SetAccessAsync(
			"~123",
			new CaseAccessRequest { Access = new Access { Kind = AccessKind.UserAccessKind, Users = ["bob@example.com"] } },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/access");
		stub.Calls[0].Body.Should().Be("""{"access":{"_kind":"UserAccessKind","users":["bob@example.com"]}}""");
	}

	[Fact]
	public async Task BulkSetAccessAsync_PostsIdsAndAccess()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.BulkSetAccessAsync(
			new CaseBulkAccessRequest { Ids = ["~1", "2"], Access = new Access { Kind = AccessKind.OrganisationAccessKind } },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/_bulk/access");
		stub.Calls[0].Body.Should().Be("""{"ids":["~1","2"],"access":{"_kind":"OrganisationAccessKind"}}""");
	}

	[Fact]
	public async Task BulkApplyTemplateAsync_PostsEveryFieldWithWireNames()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new CaseBulkApplyTemplateRequest
		{
			Ids = ["~128458762"],
			CaseTemplate = "Ransomware",
			UpdateTitlePrefix = true,
			UpdateDescription = false,
			UpdateTags = true,
			UpdateSeverity = false,
			UpdateFlag = true,
			UpdateTlp = false,
			UpdatePap = true,
			UpdateCustomFields = true,
			ImportTasks = ["~123456789"],
			ImportPages = ["~111111111"]
		};

		await client.Cases.BulkApplyTemplateAsync(request, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/_bulk/caseTemplate");
		stub.Calls[0].Body.Should().Be(
			"""{"ids":["~128458762"],"caseTemplate":"Ransomware","updateTitlePrefix":true,"updateDescription":false,"updateTags":true,"updateSeverity":false,"updateFlag":true,"updateTlp":false,"updatePap":true,"updateCustomFields":true,"importTasks":["~123456789"],"importPages":["~111111111"]}""");
	}

	[Fact]
	public async Task BulkApplyTemplateAsync_OmitsUnsetOptions()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.BulkApplyTemplateAsync(
			new CaseBulkApplyTemplateRequest { Ids = ["~1"], CaseTemplate = "Phishing" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"ids":["~1"],"caseTemplate":"Phishing"}""");
	}

	[Fact]
	public async Task ChangeOwnerAsync_PostsEveryField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new CaseOwnerChangeRequest
		{
			Organisation = "TheOrganization",
			KeepProfile = "analyst",
			TaskRule = SharingRule.AutoShare,
			ObservableRule = SharingRule.Manual
		};

		await client.Cases.ChangeOwnerAsync("~123", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/owner");
		stub.Calls[0].Body.Should().Be("""{"organisation":"TheOrganization","keepProfile":"analyst","taskRule":"autoShare","observableRule":"manual"}""");
	}

	[Fact]
	public async Task ChangeOwnerAsync_OrganisationOnly_OmitsTheRest()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.ChangeOwnerAsync("7", new CaseOwnerChangeRequest { Organisation = "Org" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"organisation":"Org"}""");
	}

	[Fact]
	public async Task RemoveAlertAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.RemoveAlertAsync("~123", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/alert/~456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeduplicateObservablesAsync_PostsAndMapsCounts()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"untouched":14,"updated":3,"deleted":5}""");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.DeduplicateObservablesAsync("~123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/observable/_merge");
		stub.Calls[0].Body.Should().BeNull();
		result.Untouched.Should().Be(14);
		result.Updated.Should().Be(3);
		result.Deleted.Should().Be(5);
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task CaseLinks_AddAndRemove_PostTypeAndCaseId(bool add)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new CaseLinkRequest { Type = "Duplicate", CaseId = "~72637286" };

		await (add
			? client.Cases.AddCaseLinkAsync("~123", request, TestContext.Current.CancellationToken)
			: client.Cases.RemoveCaseLinkAsync("~123", request, TestContext.Current.CancellationToken));

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be(add ? "/api/v1/case/~123/link/case/add" : "/api/v1/case/~123/link/case/remove");
		stub.Calls[0].Body.Should().Be("""{"type":"Duplicate","caseId":"~72637286"}""");
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task ExternalLinks_AddAndRemove_PostTypeAndUrl(bool add)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);
		var request = new ExternalLinkRequest { Type = "MITRE ATT&CK", Url = "https://attack.mitre.org/techniques/T1486/" };

		await (add
			? client.Cases.AddExternalLinkAsync("~123", request, TestContext.Current.CancellationToken)
			: client.Cases.RemoveExternalLinkAsync("~123", request, TestContext.Current.CancellationToken));

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be(add ? "/api/v1/case/~123/link/external/add" : "/api/v1/case/~123/link/external/remove");
		// The default encoder escapes '&' as a \u escape, so compare the parsed members (names, order and values).
		using var body = JsonDocument.Parse(stub.Calls[0].Body!);
		body.RootElement.EnumerateObject().Select(p => $"{p.Name}={p.Value.GetString()}")
			.Should().Equal("type=MITRE ATT&CK", "url=https://attack.mitre.org/techniques/T1486/");
	}

	[Fact]
	public async Task GetLinkTypesAsync_MapsNames()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """["Duplicate","MITRE ATT&CK"]""");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetLinkTypesAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/link/types");
		result.Should().Equal("Duplicate", "MITRE ATT&CK");
	}

	[Fact]
	public async Task DeleteCustomFieldAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteCustomFieldAsync("~9", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/customField/~9");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetSimilarObservablesAsync_MapsEveryObservableField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $"[{ObservableJson},{MinimalObservableJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetSimilarObservablesAsync("~123", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/similar/~456/observables");
		result.Should().HaveCount(2);
		var full = result[0];
		full.Id.Should().Be("~8529344");
		full.Type.Should().Be("Observable");
		full.CreatedBy.Should().Be("lucas@example.com");
		full.UpdatedBy.Should().Be("alice@example.com");
		full.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		full.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		full.DataType.Should().Be("ip");
		full.Data.Should().Be("00.01.002.003");
		full.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		AssertFullAttachment(full.Attachment!);
		full.Tlp.Should().Be(Tlp.Amber);
		full.TlpLabel.Should().Be("AMBER");
		full.Pap.Should().Be(Pap.Red);
		full.PapLabel.Should().Be("RED");
		full.Tags.Should().Equal("Source IP");
		full.Ioc.Should().BeTrue();
		full.Sighted.Should().BeTrue();
		full.SightedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748822400000));
		full.Reports["VirusTotal_GetReport"].GetProperty("status").GetString().Should().Be("Success");
		full.Message.Should().Be("Source IP of the ransomware C2 server");
		full.ExtraData["seen"].GetInt32().Should().Be(2);
		full.IgnoreSimilarity.Should().BeTrue();
		full.External.Should().BeTrue();
		var minimal = result[1];
		minimal.UpdatedBy.Should().BeNull();
		minimal.UpdatedAt.Should().BeNull();
		minimal.Data.Should().BeNull();
		minimal.Attachment.Should().BeNull();
		minimal.Tags.Should().BeEmpty();
		minimal.SightedAt.Should().BeNull();
		minimal.Message.Should().BeNull();
		minimal.Reports.Should().BeEmpty();
	}

	[Fact]
	public void Observable_Defaults_AreEmptyNotNull()
	{
		var item = new Observable();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.DataType.Should().BeEmpty();
		item.TlpLabel.Should().BeEmpty();
		item.PapLabel.Should().BeEmpty();
		item.Tags.Should().BeEmpty();
		item.Reports.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task GetTimelineAsync_MapsEveryEventField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """
			{"events":[
				{"date":1736849400000,"kind":"task","entity":"Task","entityId":"~72637286",
				 "details":{"task":{"title":"Isolate affected workstation from the network","status":"InProgress"}},"endDate":1736939400000},
				{"date":1736849300000,"kind":"case.inProgress","entity":"Case","entityId":"~1","details":{}},
				{"date":1736849200000,"kind":"case.archived","entity":"Dossier","entityId":"~2","details":{}}
			]}
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetTimelineAsync("~123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/timeline");
		result.Events.Should().HaveCount(3);
		var task = result.Events[0];
		task.Date.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1736849400000));
		task.Kind.Should().Be(TimelineEventKind.Task);
		task.Entity.Should().Be(TimelineEntityType.Task);
		task.EntityId.Should().Be("~72637286");
		task.Details["task"].GetProperty("status").GetString().Should().Be("InProgress");
		task.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1736939400000));
		result.Events[1].Kind.Should().Be(TimelineEventKind.CaseInProgress);
		result.Events[1].Entity.Should().Be(TimelineEntityType.Case);
		result.Events[1].Details.Should().BeEmpty();
		result.Events[1].EndDate.Should().BeNull();
		result.Events[2].Kind.Should().Be(TimelineEventKind.Unknown);
		result.Events[2].Entity.Should().Be(TimelineEntityType.Unknown);
	}

	[Theory]
	[InlineData("case.start", TimelineEventKind.CaseStart)]
	[InlineData("case.created", TimelineEventKind.CaseCreated)]
	[InlineData("case.new", TimelineEventKind.CaseNew)]
	[InlineData("case.inProgress", TimelineEventKind.CaseInProgress)]
	[InlineData("case.closed", TimelineEventKind.CaseClosed)]
	[InlineData("case.end", TimelineEventKind.CaseEnd)]
	[InlineData("alert.occurred", TimelineEventKind.AlertOccurred)]
	[InlineData("procedure.occurred", TimelineEventKind.ProcedureOccurred)]
	[InlineData("observable.sighted", TimelineEventKind.ObservableSighted)]
	[InlineData("task", TimelineEventKind.Task)]
	[InlineData("log.created", TimelineEventKind.LogCreated)]
	[InlineData("custom", TimelineEventKind.Custom)]
	public void TimelineEventKind_ReadsEveryWireName(string wire, TimelineEventKind expected) =>
		JsonSerializer.Deserialize<TimelineEventKind>($"\"{wire}\"", TheHiveJson.Options).Should().Be(expected);

	[Theory]
	[InlineData("Case", TimelineEntityType.Case)]
	[InlineData("Alert", TimelineEntityType.Alert)]
	[InlineData("Procedure", TimelineEntityType.Procedure)]
	[InlineData("Observable", TimelineEntityType.Observable)]
	[InlineData("Task", TimelineEntityType.Task)]
	[InlineData("Log", TimelineEntityType.Log)]
	[InlineData("CustomEvent", TimelineEntityType.CustomEvent)]
	public void TimelineEntityType_ReadsEveryWireName(string wire, TimelineEntityType expected) =>
		JsonSerializer.Deserialize<TimelineEntityType>($"\"{wire}\"", TheHiveJson.Options).Should().Be(expected);

	[Fact]
	public void CaseTimeline_Defaults_AreEmptyNotNull()
	{
		new CaseTimeline().Events.Should().BeEmpty();
		var item = new TimelineEvent();
		item.EntityId.Should().BeEmpty();
		item.Details.Should().BeEmpty();
	}

	[Fact]
	public async Task AddAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);
		using var second = new MemoryStream([4, 5]);

		var result = await client.Cases.AddAttachmentsAsync(
			"~123",
			[new StreamPart(first, "encrypt.ps1", "application/x-powershell"), new StreamPart(second, "notes.txt", "text/plain")],
			canRename: true,
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachments");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(3);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "encrypt.ps1" && p.ContentType == "application/x-powershell");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "notes.txt" && p.ContentType == "text/plain");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		call.Parts[2].Name.Should().Be("canRename");
		call.Parts[2].FileName.Should().BeNull();
		call.Parts[2].Text.Should().Be("true");
		AssertFullAttachment(result.Attachments.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task AddAttachmentsAsync_WithoutCanRename_SendsOnlyFiles()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Cases.AddAttachmentsAsync(
			"~123",
			[new ByteArrayPart([9], "a.bin")],
			cancellationToken: TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("attachments");
		part.FileName.Should().Be("a.bin");
		part.Bytes.Should().Equal(9);
	}

	[Fact]
	public void Attachment_Defaults_AreEmptyNotNull()
	{
		var item = new Attachment();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Hashes.Should().BeEmpty();
		item.ContentType.Should().BeEmpty();
		item.StorageId.Should().BeEmpty();
		item.Path.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
		new AttachmentUploadResult().Attachments.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAttachmentAsync_PatchesExternalFlag()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.UpdateAttachmentAsync("~123", "~456", new AttachmentUpdateRequest { External = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachment/~456");
		stub.Calls[0].Body.Should().Be("""{"external":true}""");
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteAttachmentAsync("~123", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachment/~456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ExportAsync_ReturnsArchiveBytesAndFileName()
	{
		var stub = new StubHandler();
		byte[] archive = [0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF];
		stub.EnqueueFile(archive, "application/octet-stream", "7.thar");
		using var client = TestClient.Create(stub);

		using var content = await client.Cases.ExportAsync("~123", "fake pass", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/export");
		stub.Calls[0].Uri.Query.Should().Be("?password=fake%20pass");
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("7.thar");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(archive);
	}

	[Fact]
	public async Task ExportAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Your licence does not allow this"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Cases.ExportAsync("~123", "pw", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}

	[Fact]
	public async Task ImportAsync_UploadsJsonAndFilePartsAndMapsResult()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $$"""
			{
				"case":{{FullCaseJson}},
				"observables":[{{MinimalObservableJson}}],
				"procedures":[{
					"_id":"~234567890","_createdAt":1748739600000,"_createdBy":"lucas@example.com","_updatedAt":1776902400000,
					"_updatedBy":"alice@example.com","description":"PowerShell script used to encrypt files.","occurDate":1748739600000,
					"patternId":"T1486","patternName":"Data Encrypted for Impact","tactic":"impact","tacticLabel":"Impact","extraData":{"k":1}
				}],
				"errors":[{"message":"observable skipped"}]
			}
			""");
		using var client = TestClient.Create(stub);
		using var archive = new MemoryStream([7, 8, 9]);
		var request = new CaseImportRequest
		{
			Password = "fake-password",
			SharingParameters = [new ShareSettings { Organisation = "Org" }],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.AutoShare
		};

		var result = await client.Cases.ImportAsync(request, new StreamPart(archive, "7.thar", "application/octet-stream"), TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/case/import");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Name.Should().Be("_json");
		call.Parts[0].FileName.Should().BeNull();
		call.Parts[0].Text.Should().Be(
			"""{"password":"fake-password","sharingParameters":[{"organisation":"Org"}],"taskRule":"manual","observableRule":"autoShare"}""");
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "file" && p.FileName == "7.thar" && p.ContentType == "application/octet-stream");
		call.Parts[1].Bytes.Should().Equal(7, 8, 9);
		result.Case.Id.Should().Be("~123");
		result.Observables.Should().ContainSingle().Which.Id.Should().Be("~1");
		var procedure = result.Procedures.Should().ContainSingle().Subject;
		procedure.Id.Should().Be("~234567890");
		procedure.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		procedure.CreatedBy.Should().Be("lucas@example.com");
		procedure.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		procedure.UpdatedBy.Should().Be("alice@example.com");
		procedure.Description.Should().Be("PowerShell script used to encrypt files.");
		procedure.OccurDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		procedure.PatternId.Should().Be("T1486");
		procedure.PatternName.Should().Be("Data Encrypted for Impact");
		procedure.Tactic.Should().Be("impact");
		procedure.TacticLabel.Should().Be("Impact");
		procedure.ExtraData["k"].GetInt32().Should().Be(1);
		result.Errors.Should().ContainSingle().Which.GetProperty("message").GetString().Should().Be("observable skipped");
	}

	[Fact]
	public async Task ImportAsync_PasswordOnly_SendsMinimalJson()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $$"""{"case":{{MinimalCaseJson}}}""");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.ImportAsync(
			new CaseImportRequest { Password = "pw" },
			new ByteArrayPart([1], "a.thar"),
			TestContext.Current.CancellationToken);

		stub.Calls[0].Parts[0].Text.Should().Be("""{"password":"pw"}""");
		result.Observables.Should().BeEmpty();
		result.Procedures.Should().BeEmpty();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void ImportResultAndProcedure_Defaults_AreEmptyNotNull()
	{
		new CaseImportResult().Case.Id.Should().BeEmpty();
		var procedure = new Procedure();
		procedure.Id.Should().BeEmpty();
		procedure.CreatedBy.Should().BeEmpty();
		procedure.ExtraData.Should().BeEmpty();
		procedure.UpdatedBy.Should().BeNull();
		procedure.Description.Should().BeNull();
	}

	private static void AssertFullAttachment(Attachment attachment)
	{
		attachment.Id.Should().Be("~456789012");
		attachment.Type.Should().Be("Attachment");
		attachment.CreatedBy.Should().Be("lucas@example.com");
		attachment.UpdatedBy.Should().Be("alice@example.com");
		attachment.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		attachment.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		attachment.Name.Should().Be("encrypt.ps1");
		attachment.Hashes.Should().Equal("fake-hash-0001", "fake-hash-0002");
		attachment.Size.Should().Be(2048);
		attachment.ContentType.Should().Be("application/x-powershell");
		attachment.StorageId.Should().Be("fake-storage-id");
		attachment.Path.Should().Be("attachments/fake-storage-id");
		attachment.ExtraData["links"].GetInt32().Should().Be(1);
		attachment.External.Should().BeTrue();
	}
}
