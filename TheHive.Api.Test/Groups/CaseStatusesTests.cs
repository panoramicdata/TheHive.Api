using System.Net;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.CaseStatuses;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CaseStatusesTests
{
	private const string FullStatusJson = """
		{
			"_id":"~84123","_type":"CaseStatus","_updatedAt":1718723423000,"_updatedBy":"emma@example.com",
			"_createdAt":1718532000000,"_createdBy":"emma@example.com","value":"Contained","stage":"Closed","order":2,
			"description":"Case has been contained but investigation continues.","colour":"#ffa940","hidden":true,
			"extraData":{"usage":4}
		}
		""";

	private const string MinimalStatusJson = """
		{"_id":"~1","_type":"CaseStatus","_createdAt":1718532000000,"_createdBy":"a@example.com","value":"v","stage":"Wibble","hidden":false,"extraData":{}}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullStatusJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseStatuses.CreateAsync(
			new CaseStatusCreateRequest
			{
				Value = "Contained",
				Stage = CaseStage.Closed,
				Order = 2,
				Description = "Case has been contained but investigation continues.",
				Colour = "#ffa940",
				Hidden = false
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseStatus");
		stub.Calls[0].Body.Should().Be(
			"""{"value":"Contained","stage":"Closed","order":2,"description":"Case has been contained but investigation continues.","colour":"#ffa940","hidden":false}""");
		result.Id.Should().Be("~84123");
		result.Type.Should().Be("CaseStatus");
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718723423000));
		result.UpdatedBy.Should().Be("emma@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000000));
		result.CreatedBy.Should().Be("emma@example.com");
		result.Value.Should().Be("Contained");
		result.Stage.Should().Be(CaseStage.Closed);
		result.Order.Should().Be(2);
		result.Description.Should().Be("Case has been contained but investigation continues.");
		result.Colour.Should().Be("#ffa940");
		result.Hidden.Should().BeTrue();
		result.ExtraData["usage"].GetInt32().Should().Be(4);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalStatusJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseStatuses.CreateAsync(
			new CaseStatusCreateRequest { Value = "v", Stage = CaseStage.InProgress },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"value":"v","stage":"InProgress"}""");
		result.Stage.Should().Be(CaseStage.Unknown);
		result.UpdatedAt.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.Order.Should().BeNull();
		result.Description.Should().BeNull();
		result.Colour.Should().BeNull();
		result.Hidden.Should().BeFalse();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void CaseStatus_Defaults_AreEmptyNotNull()
	{
		var item = new CaseStatus();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Value.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryField()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseStatuses.UpdateAsync(
			"Contained",
			new CaseStatusUpdateRequest { Order = 3, Description = "d", Colour = "#000000", Hidden = true },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseStatus/Contained");
		stub.Calls[0].Body.Should().Be("""{"order":3,"description":"d","colour":"#000000","hidden":true}""");
	}

	[Fact]
	public async Task UpdateAsync_ExplicitNulls_SendNullToClear()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseStatuses.UpdateAsync(
			"~84123",
			new CaseStatusUpdateRequest { Order = null, Description = null, Colour = null },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseStatus/~84123");
		stub.Calls[0].Body.Should().Be("""{"order":null,"description":null,"colour":null}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseStatuses.UpdateAsync("~84123", new CaseStatusUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseStatuses.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseStatus/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_StillAssigned_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Status is in use"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.CaseStatuses.DeleteAsync("Open", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
