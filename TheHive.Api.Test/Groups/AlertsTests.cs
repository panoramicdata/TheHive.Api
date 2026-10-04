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
	private const string FullAlertJson = """
		{
			"_id":"~354","_type":"Alert","_createdBy":"lucas@example.com","_updatedBy":"emma@example.com",
			"_createdAt":1718532000000,"_updatedAt":1718535600000,"type":"Endpoint Detection","source":"EDR",
			"sourceRef":"MDATP-2025-004512","externalLink":"https://defender.microsoft.com/alerts/MDATP-2025-004512",
			"title":"File encrypted on CORP-LAPTOP-056","description":"File encryption detected via PowerShell.",
			"severity":3,"severityLabel":"HIGH","date":1718531000000,"tags":["threat-type:file-encryption","TheOrganization"],
			"tlp":2,"tlpLabel":"AMBER","pap":1,"papLabel":"GREEN","follow":true,
			"customFields":[{"_id":"~9","name":"threat-type","type":"string","value":"Malware","order":0}],
			"caseTemplate":"Ransomware Investigation","observableCount":4,"caseId":"~216513541",
			"status":"Imported","stage":"Imported","assignee":"lucas@example.com","summary":"Three workstations.",
			"extraData":{"score":5},"newDate":1718532000001,"inProgressDate":1718532000002,"closedDate":1718532000003,
			"importedDate":1718532000004,"timeToDetect":11,"timeToTriage":12,"timeToQualify":13,"timeToAcknowledge":14
		}
		""";

	private const string MinimalAlertJson = """
		{
			"_id":"~355","_type":"Alert","_createdBy":"lucas@example.com","_createdAt":1718532000000,"type":"t",
			"source":"s","sourceRef":"r","title":"x","description":"d","severity":2,"severityLabel":"MEDIUM",
			"date":1718532000000,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","follow":false,
			"observableCount":0,"status":"Archived2099","stage":"Archived2099","extraData":{},
			"newDate":1718532000000,"timeToDetect":0
		}
		""";

	private const string CaseJson = """
		{
			"_id":"~123","_type":"Case","_createdBy":"alice@example.com","_createdAt":1700000000000,"number":7,
			"title":"Ransomware","description":"d","severity":2,"severityLabel":"MEDIUM","startDate":1700000000000,
			"flag":false,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","status":"New",
			"stage":"New","access":{"_kind":"OrganisationAccessKind"},"extraData":{},
			"newDate":1700000000000,"timeToDetect":0
		}
		""";

	private const string AttachmentJson = """
		{
			"_id":"~456789012","_type":"Attachment","_createdBy":"lucas@example.com","_createdAt":1748739600000,
			"name":"encrypt.ps1","hashes":["fake-hash-0001"],"size":2048,
			"contentType":"application/x-powershell","id":"fake-storage-id",
			"path":"attachments/fake-storage-id","extraData":{},"external":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullAlert(Alert result)
	{
		result.Id.Should().Be("~354");
		result.EntityType.Should().Be("Alert");
		result.CreatedBy.Should().Be("lucas@example.com");
		result.UpdatedBy.Should().Be("emma@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718535600000));
		result.Type.Should().Be("Endpoint Detection");
		result.Source.Should().Be("EDR");
		result.SourceRef.Should().Be("MDATP-2025-004512");
		result.ExternalLink.Should().Be("https://defender.microsoft.com/alerts/MDATP-2025-004512");
		result.Title.Should().Be("File encrypted on CORP-LAPTOP-056");
		result.Description.Should().Be("File encryption detected via PowerShell.");
		result.Severity.Should().Be(Severity.High);
		result.SeverityLabel.Should().Be("HIGH");
		result.Date.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718531000000));
		result.Tags.Should().Equal("threat-type:file-encryption", "TheOrganization");
		result.Tlp.Should().Be(Tlp.Amber);
		result.TlpLabel.Should().Be("AMBER");
		result.Pap.Should().Be(Pap.Green);
		result.PapLabel.Should().Be("GREEN");
		result.Follow.Should().BeTrue();
		var field = result.CustomFields.Should().ContainSingle().Subject;
		field.Id.Should().Be("~9");
		field.Name.Should().Be("threat-type");
		field.Type.Should().Be("string");
		field.Value.GetString().Should().Be("Malware");
		field.Order.Should().Be(0);
		result.CaseTemplate.Should().Be("Ransomware Investigation");
		result.ObservableCount.Should().Be(4);
		result.CaseId.Should().Be("~216513541");
		result.Status.Should().Be("Imported");
		result.Stage.Should().Be(AlertStage.Imported);
		result.Assignee.Should().Be("lucas@example.com");
		result.Summary.Should().Be("Three workstations.");
		result.ExtraData["score"].GetInt32().Should().Be(5);
		result.NewDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000001));
		result.InProgressDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000002));
		result.ClosedDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000003));
		result.ImportedDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000004));
		result.TimeToDetect.Should().Be(11);
		result.TimeToTriage.Should().Be(12);
		result.TimeToQualify.Should().Be(13);
		result.TimeToAcknowledge.Should().Be(14);
	}

	[Fact]
	public async Task GetAsync_MapsEveryAlertField()
	{
		var stub = Stub(HttpStatusCode.OK, FullAlertJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.GetAsync("~354", TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullAlert(result);
	}

	[Fact]
	public async Task GetAsync_UnknownStage_And_AbsentOptionals()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalAlertJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.GetAsync("~355", TestContext.Current.CancellationToken);

		result.Status.Should().Be("Archived2099");
		result.Stage.Should().Be(AlertStage.Unknown);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.ExternalLink.Should().BeNull();
		result.CaseTemplate.Should().BeNull();
		result.CaseId.Should().BeNull();
		result.Assignee.Should().BeNull();
		result.Summary.Should().BeNull();
		result.InProgressDate.Should().BeNull();
		result.ClosedDate.Should().BeNull();
		result.ImportedDate.Should().BeNull();
		result.TimeToTriage.Should().BeNull();
		result.TimeToQualify.Should().BeNull();
		result.TimeToAcknowledge.Should().BeNull();
		result.Tags.Should().BeEmpty();
		result.CustomFields.Should().BeEmpty();
		result.ExtraData.Should().BeEmpty();
	}

	[Theory]
	[InlineData("New", AlertStage.New)]
	[InlineData("InProgress", AlertStage.InProgress)]
	[InlineData("Closed", AlertStage.Closed)]
	[InlineData("Imported", AlertStage.Imported)]
	public void AlertStage_ReadsEveryWireName(string wire, AlertStage expected) =>
		JsonSerializer.Deserialize<AlertStage>($"\"{wire}\"", TheHiveJson.Options).Should().Be(expected);

	[Fact]
	public void Alert_Defaults_AreEmptyNotNull()
	{
		var item = new Alert();

		item.Id.Should().BeEmpty();
		item.EntityType.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.Source.Should().BeEmpty();
		item.SourceRef.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.SeverityLabel.Should().BeEmpty();
		item.TlpLabel.Should().BeEmpty();
		item.PapLabel.Should().BeEmpty();
		item.Status.Should().BeEmpty();
		item.Tags.Should().BeEmpty();
		item.CustomFields.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateAsync_PostsRequiredFieldsOnlyAndMapsResult()
	{
		var stub = Stub(HttpStatusCode.Created, FullAlertJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.CreateAsync(
			new AlertCreateRequest { Type = "Endpoint Detection", Source = "EDR", SourceRef = "MDATP-1", Title = "t", Description = "d" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be("""{"type":"Endpoint Detection","source":"EDR","sourceRef":"MDATP-1","title":"t","description":"d"}""");
		result.Id.Should().Be("~354");
	}

	private static AlertCreateRequest FullCreateRequest() => new()
	{
		Type = "Endpoint Detection",
		Source = "EDR",
		SourceRef = "MDATP-1",
		ExternalLink = "https://example.com/a",
		Title = "t",
		Description = "d",
		Severity = Severity.Critical,
		Date = DateTimeOffset.FromUnixTimeMilliseconds(1718532000000),
		Tags = ["x"],
		Flag = true,
		Tlp = Tlp.Red,
		Pap = Pap.Red,
		CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware", Order = 0 }],
		Summary = "sum",
		Status = "New",
		Assignee = "lucas@example.com",
		CaseTemplate = "Ransomware",
		Observables =
		[
			new ObservableInput
			{
				DataType = "file",
				Data = ["a", "b"],
				Message = "m",
				StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1),
				Attachment = [new ObservableAttachmentReference { Name = "x.exe", ContentType = "application/octet-stream", Id = "~1", External = true }],
				Tlp = Tlp.Green,
				Pap = Pap.Green,
				Tags = ["t"],
				Ioc = true,
				Sighted = false,
				SightedAt = DateTimeOffset.FromUnixTimeMilliseconds(2),
				IgnoreSimilarity = true,
				IsZip = true,
				ZipPassword = "infected"
			},
			new ObservableInput { DataType = "hostname", Data = ["CORP-LAPTOP-056"] }
		],
		Procedures =
		[
			new ProcedureInput { PatternId = "T1486", OccurDate = DateTimeOffset.FromUnixTimeMilliseconds(3), Tactic = "impact", Description = "pd" },
			new ProcedureInput { PatternId = "T1059", OccurDate = DateTimeOffset.FromUnixTimeMilliseconds(4) }
		]
	};

	[Fact]
	public async Task CreateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = Stub(HttpStatusCode.Created, FullAlertJson);
		using var client = TestClient.Create(stub);
		var request = FullCreateRequest();

		await client.Alerts.CreateAsync(request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			"""{"type":"Endpoint Detection","source":"EDR","sourceRef":"MDATP-1","externalLink":"https://example.com/a","title":"t","description":"d","severity":4,"date":1718532000000,"tags":["x"],"flag":true,"tlp":4,"pap":3,"customFields":[{"name":"threat-type","value":"Malware","order":0}],"summary":"sum","status":"New","assignee":"lucas@example.com","caseTemplate":"Ransomware","observables":[{"dataType":"file","data":["a","b"],"message":"m","startDate":1,"attachment":[{"name":"x.exe","contentType":"application/octet-stream","id":"~1","external":true}],"tlp":1,"pap":1,"tags":["t"],"ioc":true,"sighted":false,"sightedAt":2,"ignoreSimilarity":true,"isZip":true,"zipPassword":"infected"},{"dataType":"hostname","data":["CORP-LAPTOP-056"]}],"procedures":[{"patternId":"T1486","occurDate":3,"tactic":"impact","description":"pd"},{"patternId":"T1059","occurDate":4}]}""");
	}

	[Fact]
	public async Task UpdateAsync_PatchesOnlySetFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.UpdateAsync("~354", new AlertUpdateRequest { Title = "New" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354");
		stub.Calls[0].Body.Should().Be("""{"title":"New"}""");
	}

	private static AlertUpdateRequest FullUpdate() => new()
	{
		Type = "t",
		Source = "s",
		SourceRef = "r",
		ExternalLink = "https://example.com/a",
		Title = "title",
		Description = "d",
		Severity = Severity.Low,
		Date = DateTimeOffset.FromUnixTimeMilliseconds(10),
		LastSyncDate = DateTimeOffset.FromUnixTimeMilliseconds(20),
		Tags = ["a"],
		Tlp = Tlp.Clear,
		Pap = Pap.Amber,
		Follow = false,
		CustomFields = [new CustomFieldInput { Name = "severity", Value = 3 }],
		Status = "InProgress",
		Summary = "s",
		Assignee = "sami@example.com",
		AddTags = ["ransomware"],
		RemoveTags = ["old"]
	};

	private const string FullUpdateJson = """
		"type":"t","source":"s","sourceRef":"r","externalLink":"https://example.com/a","title":"title","description":"d","severity":1,"date":10,"lastSyncDate":20,"tags":["a"],"tlp":0,"pap":2,"follow":false,"customFields":[{"name":"severity","value":3}],"status":"InProgress","summary":"s","assignee":"sami@example.com","addTags":["ransomware"],"removeTags":["old"]
		""";

	[Fact]
	public async Task UpdateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.UpdateAsync("~354", FullUpdate(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{" + FullUpdateJson + "}");
	}

	[Theory]
	[InlineData("externalLink")]
	[InlineData("summary")]
	[InlineData("assignee")]
	public async Task UpdateAsync_ExplicitNull_SendsNullToUnset(string field)
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = field switch
		{
			"externalLink" => new AlertUpdateRequest { ExternalLink = null },
			"summary" => new AlertUpdateRequest { Summary = null },
			_ => new AlertUpdateRequest { Assignee = null }
		};

		await client.Alerts.UpdateAsync("~354", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"{{field}}":null}""");
	}

	[Fact]
	public void AlertUpdateRequest_ClearableFields_DefaultToUnset()
	{
		var request = new AlertUpdateRequest();

		request.ExternalLink.HasValue.Should().BeFalse();
		request.Summary.HasValue.Should().BeFalse();
		request.Assignee.HasValue.Should().BeFalse();
	}
}
