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

public partial class CasesTests
{
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
}
