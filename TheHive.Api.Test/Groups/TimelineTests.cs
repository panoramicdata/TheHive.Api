using System.Net;
using TheHive.Api.Data.Timeline;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class TimelineTests
{
	private const string EventJson = """
		{
			"_id":"~24568324","_type":"CustomEvent","_createdBy":"lucas@example.com","_updatedBy":"emma@example.com",
			"_createdAt":1737020400000,"_updatedAt":1737025800000,"date":1737020400000,"endDate":1737025800000,
			"title":"Ransom demand received via email",
			"description":"Attacker sent a ransom demand to the security team."
		}
		""";

	private const string MinimalEventJson = """
		{
			"_id":"~1","_type":"CustomEvent","_createdBy":"lucas@example.com","_createdAt":1737020400000,
			"date":1737020400000,"title":"t"
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task CreateCustomEventAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, EventJson);
		using var client = TestClient.Create(stub);

		var result = await client.Timeline.CreateCustomEventAsync(
			"~123",
			new CustomEventCreateRequest
			{
				Date = DateTimeOffset.FromUnixTimeMilliseconds(1737020400000),
				EndDate = DateTimeOffset.FromUnixTimeMilliseconds(1737025800000),
				Title = "Ransom demand received via email",
				Description = "Attacker sent a ransom demand to the security team."
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/customEvent");
		stub.Calls[0].Body.Should().Be(
			"""{"date":1737020400000,"endDate":1737025800000,"title":"Ransom demand received via email","description":"Attacker sent a ransom demand to the security team."}""");
		result.Id.Should().Be("~24568324");
		result.Type.Should().Be("CustomEvent");
		result.CreatedBy.Should().Be("lucas@example.com");
		result.UpdatedBy.Should().Be("emma@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1737020400000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1737025800000));
		result.Date.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1737020400000));
		result.EndDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1737025800000));
		result.Title.Should().Be("Ransom demand received via email");
		result.Description.Should().Be("Attacker sent a ransom demand to the security team.");
	}

	[Fact]
	public async Task CreateCustomEventAsync_RequiredOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalEventJson);
		using var client = TestClient.Create(stub);

		var result = await client.Timeline.CreateCustomEventAsync(
			"~123",
			new CustomEventCreateRequest { Date = DateTimeOffset.FromUnixTimeMilliseconds(1737020400000), Title = "t" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"date":1737020400000,"title":"t"}""");
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.EndDate.Should().BeNull();
		result.Description.Should().BeNull();
	}

	[Fact]
	public void CustomEvent_Defaults_AreEmptyNotNull()
	{
		var item = new CustomEvent();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateCustomEventAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Timeline.UpdateCustomEventAsync(
			"~24568324",
			new CustomEventUpdateRequest
			{
				Date = DateTimeOffset.FromUnixTimeMilliseconds(1737020400000),
				EndDate = DateTimeOffset.FromUnixTimeMilliseconds(1737025800000),
				Title = "New title",
				Description = "New description"
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customEvent/~24568324");
		stub.Calls[0].Body.Should().Be("""{"date":1737020400000,"endDate":1737025800000,"title":"New title","description":"New description"}""");
	}

	[Fact]
	public async Task UpdateCustomEventAsync_NullEndDateAndDescription_SendExplicitNulls()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Timeline.UpdateCustomEventAsync(
			"~24568324",
			new CustomEventUpdateRequest { EndDate = null, Description = null },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"endDate":null,"description":null}""");
	}

	[Fact]
	public async Task UpdateCustomEventAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Timeline.UpdateCustomEventAsync("~24568324", new CustomEventUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteCustomEventAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Timeline.DeleteCustomEventAsync("~24568324", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customEvent/~24568324");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteCustomEventAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"CustomEvent ~1 not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Timeline.DeleteCustomEventAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
