using System.Net;
using TheHive.Api.Data.AuditTrail;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class AuditTests
{
	private const string FlowJson = """
		[
			{
				"base":{
					"_id":"~327684328","_type":"Audit","_createdBy":"alice@example.com","_updatedBy":"sami@example.com",
					"_createdAt":1694441999960,"_updatedAt":1694442000000,"action":"update",
					"requestId":"74dc37479904ebe7:3957d351:18a847d4266:-8000:109","rootId":"~327925760",
					"details":{"status":"InProgress","stage":"InProgress"},"objectId":"~327925761","objectType":"Case",
					"object":{"_id":"~327925760","title":"Suspicious network activity detected"},
					"context":{"_id":"~327925760","status":"InProgress"},"mainAction":true
				},
				"summary":{"Task":{"create":2},"Case":{"update":1,"merge":3}}
			},
			{
				"base":{
					"_id":"~2","_type":"Audit","_createdBy":"bob@example.com","_createdAt":1694441999960,"action":"delete",
					"requestId":"r","rootId":"~3","details":{},"mainAction":false
				},
				"summary":{}
			}
		]
		""";

	[Fact]
	public async Task GetFlowAsync_WithoutArguments_SendsNoQuery_AndMapsEveryField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, FlowJson);
		using var client = TestClient.Create(stub);

		var result = await client.Audit.GetFlowAsync(new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/flow");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
		result.Should().HaveCount(2);
		var entry = result[0].Base;
		entry.Id.Should().Be("~327684328");
		entry.Type.Should().Be("Audit");
		entry.CreatedBy.Should().Be("alice@example.com");
		entry.UpdatedBy.Should().Be("sami@example.com");
		entry.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1694441999960));
		entry.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1694442000000));
		entry.Action.Should().Be("update");
		entry.RequestId.Should().Be("74dc37479904ebe7:3957d351:18a847d4266:-8000:109");
		entry.RootId.Should().Be("~327925760");
		entry.Details!.Value.GetProperty("status").GetString().Should().Be("InProgress");
		entry.ObjectId.Should().Be("~327925761");
		entry.ObjectType.Should().Be("Case");
		entry.Object!.Value.GetProperty("title").GetString().Should().Be("Suspicious network activity detected");
		entry.Context!.Value.GetProperty("status").GetString().Should().Be("InProgress");
		entry.MainAction.Should().BeTrue();
		result[0].Summary.Should().HaveCount(2);
		result[0].Summary["Task"]["create"].Should().Be(2);
		result[0].Summary["Case"]["merge"].Should().Be(3);
	}

	[Fact]
	public async Task GetFlowAsync_AbsentOptionals_MapToDefaults()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, FlowJson);
		using var client = TestClient.Create(stub);

		var result = await client.Audit.GetFlowAsync(new(), TestContext.Current.CancellationToken);

		var entry = result[1].Base;
		entry.UpdatedBy.Should().BeNull();
		entry.UpdatedAt.Should().BeNull();
		entry.ObjectId.Should().BeNull();
		entry.ObjectType.Should().BeNull();
		entry.Object.Should().BeNull();
		entry.Context.Should().BeNull();
		entry.MainAction.Should().BeFalse();
		result[1].Summary.Should().BeEmpty();
	}

	[Fact]
	public async Task GetFlowAsync_SendsRootIdAndCount()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		var result = await client.Audit.GetFlowAsync(new AuditFlowQuery { RootId = "~327925760", Count = 25 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/flow?rootId=~327925760&count=25");
		result.Should().BeEmpty();
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var entry = new AuditEntry();
		var stream = new AuditStreamEntry();

		entry.Id.Should().BeEmpty();
		entry.Type.Should().BeEmpty();
		entry.CreatedBy.Should().BeEmpty();
		entry.Action.Should().BeEmpty();
		entry.RequestId.Should().BeEmpty();
		entry.RootId.Should().BeEmpty();
		stream.Base.Should().NotBeNull();
		stream.Summary.Should().BeEmpty();
	}

	[Fact]
	public async Task GetFlowAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Case not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Audit.GetFlowAsync(new AuditFlowQuery { RootId = "~0" }, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
