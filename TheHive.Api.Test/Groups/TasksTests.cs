using System.Net;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class TasksTests
{
	private const string FullTaskJson = """
		{
			"_id":"~84123","_type":"Task","_createdBy":"alice@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1748476800000,"_updatedAt":1748563200000,
			"title":"Isolate affected workstation from the network","group":"Containment",
			"description":"Disconnect CORP-LAPTOP-056 from the network immediately.","status":"InProgress","flag":true,
			"startDate":1748476800000,"endDate":1748563200000,"assignee":"sami@example.com","order":2,
			"dueDate":1748563200000,"mandatory":true,"extraData":{"shares":3},"timeToHandle":86400000
		}
		""";

	private const string MinimalTaskJson = """
		{
			"_id":"~1","_type":"Task","_createdBy":"alice@example.com","_createdAt":1748476800000,
			"title":"t","group":"g","status":"Waiting","flag":false,"order":0,"mandatory":false,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullTask(CaseTask task)
	{
		task.Id.Should().Be("~84123");
		task.Type.Should().Be("Task");
		task.CreatedBy.Should().Be("alice@example.com");
		task.UpdatedBy.Should().Be("lucas@example.com");
		task.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748476800000));
		task.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748563200000));
		task.Title.Should().Be("Isolate affected workstation from the network");
		task.Group.Should().Be("Containment");
		task.Description.Should().Be("Disconnect CORP-LAPTOP-056 from the network immediately.");
		task.Status.Should().Be(CaseTaskStatus.InProgress);
		task.Flag.Should().BeTrue();
		task.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748476800000));
		task.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748563200000));
		task.Assignee.Should().Be("sami@example.com");
		task.Order.Should().Be(2);
		task.DueDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748563200000));
		task.Mandatory.Should().BeTrue();
		task.ExtraData["shares"].GetInt32().Should().Be(3);
		task.TimeToHandle.Should().Be(86400000);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullTaskJson);
		using var client = TestClient.Create(stub);

		var result = await client.Tasks.CreateAsync(
			"~354",
			new CaseTaskCreateRequest
			{
				Title = "Isolate affected workstation from the network",
				Group = "Containment",
				Status = CaseTaskStatus.Waiting,
				Mandatory = true
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/task");
		stub.Calls[0].Body.Should().Be("""{"title":"Isolate affected workstation from the network","group":"Containment","status":"Waiting","mandatory":true}""");
		AssertFullTask(result);
	}

	[Fact]
	public async Task GetAsync_GetsTask_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalTaskJson);
		using var client = TestClient.Create(stub);

		var result = await client.Tasks.GetAsync("~1", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~1");
		stub.Calls[0].Body.Should().BeNull();
		result.Status.Should().Be(CaseTaskStatus.Waiting);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Description.Should().BeNull();
		result.StartDate.Should().BeNull();
		result.EndDate.Should().BeNull();
		result.Assignee.Should().BeNull();
		result.DueDate.Should().BeNull();
		result.TimeToHandle.Should().BeNull();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_FullTask_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullTaskJson);
		using var client = TestClient.Create(stub);

		AssertFullTask(await client.Tasks.GetAsync("~84123", TestContext.Current.CancellationToken));
	}

	[Fact]
	public void CaseTask_Defaults_AreEmptyNotNull()
	{
		var item = new CaseTask();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Group.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesOnlySetFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tasks.UpdateAsync("~84123", new CaseTaskUpdateRequest { Status = CaseTaskStatus.Completed }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123");
		stub.Calls[0].Body.Should().Be("""{"status":"Completed"}""");
	}

	[Fact]
	public async Task UpdateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = new CaseTaskUpdateRequest
		{
			Title = "t",
			Group = "g",
			Description = "d",
			Status = CaseTaskStatus.InProgress,
			Flag = false,
			StartDate = DateTimeOffset.FromUnixTimeMilliseconds(10),
			EndDate = DateTimeOffset.FromUnixTimeMilliseconds(20),
			Order = 1,
			DueDate = DateTimeOffset.FromUnixTimeMilliseconds(30),
			Assignee = "sami@example.com",
			Mandatory = false
		};

		await client.Tasks.UpdateAsync("~84123", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			"""{"title":"t","group":"g","description":"d","status":"InProgress","flag":false,"startDate":10,"endDate":20,"order":1,"dueDate":30,"assignee":"sami@example.com","mandatory":false}""");
	}

	[Theory]
	[InlineData("description")]
	[InlineData("startDate")]
	[InlineData("dueDate")]
	[InlineData("assignee")]
	public async Task UpdateAsync_ExplicitNull_SendsNullToUnset(string field)
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = field switch
		{
			"description" => new CaseTaskUpdateRequest { Description = null },
			"startDate" => new CaseTaskUpdateRequest { StartDate = null },
			"dueDate" => new CaseTaskUpdateRequest { DueDate = null },
			_ => new CaseTaskUpdateRequest { Assignee = null }
		};

		await client.Tasks.UpdateAsync("~84123", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"{{field}}":null}""");
	}

	[Fact]
	public async Task BulkUpdateAsync_PatchesIdsFirstThenFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tasks.BulkUpdateAsync(
			new CaseTaskBulkUpdateRequest { Title = "t", Ids = ["~128458762", "~216513541"], Assignee = null },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/_bulk");
		stub.Calls[0].Body.Should().Be("""{"ids":["~128458762","~216513541"],"title":"t","assignee":null}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tasks.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetActionRequiredAsync_MapsOrganizationFlags()
	{
		var stub = Stub(HttpStatusCode.OK, """{"TheOrganization1":true,"TheOrganization2":false}""");
		using var client = TestClient.Create(stub);

		var result = await client.Tasks.GetActionRequiredAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123/actionRequired");
		result.Should().BeEquivalentTo(new Dictionary<string, bool> { ["TheOrganization1"] = true, ["TheOrganization2"] = false });
	}

	[Fact]
	public async Task SetActionRequiredAsync_PutsToOrganization()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tasks.SetActionRequiredAsync("~84123", "The Org", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123/actionRequired/The%20Org");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task SetActionDoneAsync_PutsToOrganization()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tasks.SetActionDoneAsync("~84123", "~354", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123/actionDone/~354");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Task not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Tasks.GetAsync("~0", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e =>
				e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError" && e.Message == "Task not found");
	}
}
