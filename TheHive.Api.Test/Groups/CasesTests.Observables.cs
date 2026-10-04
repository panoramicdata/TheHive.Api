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
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"dataType":"domain","data":"c2.example.test",
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
		full.DataType.Should().Be("domain");
		full.Data.Should().Be("c2.example.test");
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
		AssertMinimalObservable(result[1]);
	}

	private static void AssertMinimalObservable(Observable minimal)
	{
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
}
