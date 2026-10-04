using Refit;
using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Procedures;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class AlertsTests
{
	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.DeleteAsync("~354", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task BulkUpdateAsync_PatchesIdsFirstThenFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = new AlertBulkUpdateRequest { Ids = ["~1", "~2"] };
		var fields = FullUpdate();
		request.Type = fields.Type;
		request.Source = fields.Source;
		request.SourceRef = fields.SourceRef;
		request.ExternalLink = null;
		request.Title = fields.Title;
		request.Description = fields.Description;
		request.Severity = fields.Severity;
		request.Date = fields.Date;
		request.LastSyncDate = fields.LastSyncDate;
		request.Tags = fields.Tags;
		request.Tlp = fields.Tlp;
		request.Pap = fields.Pap;
		request.Follow = fields.Follow;
		request.CustomFields = fields.CustomFields;
		request.Status = fields.Status;
		request.Summary = null;
		request.Assignee = null;
		request.AddTags = fields.AddTags;
		request.RemoveTags = fields.RemoveTags;

		await client.Alerts.BulkUpdateAsync(request, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/_bulk");
		stub.Calls[0].Body.Should().Be(
			"""{"ids":["~1","~2"],"type":"t","source":"s","sourceRef":"r","externalLink":null,"title":"title","description":"d","severity":1,"date":10,"lastSyncDate":20,"tags":["a"],"tlp":0,"pap":2,"follow":false,"customFields":[{"name":"severity","value":3}],"status":"InProgress","summary":null,"assignee":null,"addTags":["ransomware"],"removeTags":["old"]}""");
	}

	[Fact]
	public async Task BulkUpdateAsync_SendsOnlyIdsAndSetFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.BulkUpdateAsync(
			new AlertBulkUpdateRequest { Ids = ["~1"], Status = "Closed" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"ids":["~1"],"status":"Closed"}""");
	}

	[Fact]
	public async Task BulkDeleteAsync_PostsIds()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.BulkDeleteAsync(new AlertBulkDeleteRequest { Ids = ["~1", "~2"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/delete/_bulk");
		stub.Calls[0].Body.Should().Be("""{"ids":["~1","~2"]}""");
	}

	[Fact]
	public async Task CreateCaseAsync_PostsEveryOverrideAndMapsCase()
	{
		var stub = Stub(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);
		var request = new CaseFromAlertRequest
		{
			Title = "Suspicious Ransomware Activity",
			Description = "cd",
			Severity = Severity.High,
			StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
			EndDate = DateTimeOffset.FromUnixTimeMilliseconds(2),
			Tags = ["x"],
			Flag = true,
			Tlp = Tlp.Red,
			Pap = Pap.Red,
			Status = "New",
			Summary = "sum",
			Assignee = "lucas@example.com",
			CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware" }],
			CaseTemplate = "Ransomware",
			Tasks = [new CaseTaskCreateRequest { Title = "Isolate", Status = CaseTaskStatus.Waiting }],
			Pages = [new PageCreateRequest { Title = "Notes", Content = "c", Category = "Investigation" }],
			SharingParameters = [new ShareSettings { Organisation = "Org", Share = true }],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.AutoShare
		};

		var result = await client.Alerts.CreateCaseAsync("~354", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/case");
		stub.Calls[0].Body.Should().Be(
			"""{"title":"Suspicious Ransomware Activity","description":"cd","severity":3,"startDate":1,"endDate":2,"tags":["x"],"flag":true,"tlp":4,"pap":3,"status":"New","summary":"sum","assignee":"lucas@example.com","customFields":[{"name":"threat-type","value":"Malware"}],"caseTemplate":"Ransomware","tasks":[{"title":"Isolate","status":"Waiting"}],"pages":[{"title":"Notes","content":"c","category":"Investigation"}],"sharingParameters":[{"organisation":"Org","share":true}],"taskRule":"manual","observableRule":"autoShare"}""");
		result.Number.Should().Be(7);
		result.Title.Should().Be("Ransomware");
	}

	[Fact]
	public async Task CreateCaseAsync_EmptyRequest_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.CreateCaseAsync("~354", new CaseFromAlertRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/case");
		stub.Calls[0].Body.Should().Be("{}");
		result.Id.Should().Be("~123");
	}
}
