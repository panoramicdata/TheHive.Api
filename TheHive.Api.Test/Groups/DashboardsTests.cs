using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Dashboards;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class DashboardsTests
{
	private const string DefinitionJson = """{"period":"last3Months","items":[{"type":"container","items":[{"id":"wdgt-1a2b3c","type":"counter","options":{"title":"Open cases","entity":"case","filters":[],"series":[{"agg":"count","type":"bar","query":{},"label":"Open cases"}]}}]}]}""";

	private const string FullDashboardJson = $$"""
		{
			"_id":"~123456","_type":"Dashboard","_createdBy":"lucas@example.com","_updatedBy":"emma@example.com",
			"_createdAt":1749563414000,"_updatedAt":1749649814000,
			"title":"Cases overview","group":"Security operations",
			"description":"Overview of open cases.","status":"Shared","owner":"lucas@example.com",
			"definition":{{DefinitionJson}},"writable":true,"version":1
		}
		""";

	private const string MinimalDashboardJson = """
		{
			"_id":"~1","_type":"Dashboard","_createdBy":"lucas@example.com","_createdAt":1749563414000,
			"title":"t","group":"default","description":"d","status":"Private","writable":false,"version":1
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static JsonElement Definition() => JsonSerializer.Deserialize<JsonElement>(DefinitionJson);

	private static void AssertFullDashboard(Dashboard item)
	{
		item.Id.Should().Be("~123456");
		item.Type.Should().Be("Dashboard");
		item.CreatedBy.Should().Be("lucas@example.com");
		item.UpdatedBy.Should().Be("emma@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1749563414000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1749649814000));
		item.Title.Should().Be("Cases overview");
		item.Group.Should().Be("Security operations");
		item.Description.Should().Be("Overview of open cases.");
		item.Status.Should().Be(DashboardStatus.Shared);
		item.Owner.Should().Be("lucas@example.com");
		item.Definition.GetProperty("period").GetString().Should().Be("last3Months");
		item.Definition.GetRawText().Should().Be(DefinitionJson);
		item.Writable.Should().BeTrue();
		item.Version.Should().Be(1);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullDashboardJson);
		using var client = TestClient.Create(stub);

		var result = await client.Dashboards.CreateAsync(
			new DashboardCreateRequest
			{
				Title = "Cases overview",
				Group = "Security operations",
				Description = "Overview of open cases.",
				Status = DashboardStatus.Shared,
				Definition = Definition(),
				Version = 1
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/dashboard");
		stub.Calls[0].Body.Should().Be(
			$$"""{"title":"Cases overview","group":"Security operations","description":"Overview of open cases.","status":"Shared","definition":{{DefinitionJson}},"version":1}""");
		AssertFullDashboard(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalDashboardJson);
		using var client = TestClient.Create(stub);

		await client.Dashboards.CreateAsync(
			new DashboardCreateRequest { Title = "t", Description = "d", Status = DashboardStatus.Private, Definition = JsonSerializer.Deserialize<JsonElement>("{}") },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"title":"t","description":"d","status":"Private","definition":{}}""");
	}

	[Fact]
	public async Task GetAsync_FullDashboard_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullDashboardJson);
		using var client = TestClient.Create(stub);

		var result = await client.Dashboards.GetAsync("~123456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/dashboard/~123456");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullDashboard(result);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults_And_UnknownStatusIsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalDashboardJson.Replace("\"Private\"", "\"Archived\"", StringComparison.Ordinal));
		using var client = TestClient.Create(stub);

		var result = await client.Dashboards.GetAsync("~1", TestContext.Current.CancellationToken);

		result.Status.Should().Be(DashboardStatus.Unknown);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Owner.Should().BeNull();
		result.Definition.ValueKind.Should().Be(JsonValueKind.Undefined);
		result.Writable.Should().BeFalse();
	}

	[Fact]
	public void Dashboard_Defaults_AreEmptyNotNull()
	{
		var item = new Dashboard();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Group.Should().BeEmpty();
		item.Description.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Dashboards.UpdateAsync(
			"~123456",
			new DashboardUpdateRequest
			{
				Title = "New title",
				Group = "Ops",
				Description = "New description",
				Definition = Definition(),
				Status = DashboardStatus.Deleted,
				Version = 1
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/dashboard/~123456");
		stub.Calls[0].Body.Should().Be(
			$$"""{"title":"New title","group":"Ops","description":"New description","definition":{{DefinitionJson}},"status":"Deleted","version":1}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Dashboards.UpdateAsync("~123456", new DashboardUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Dashboards.DeleteAsync("~123456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/dashboard/~123456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ChangeOwnerAsync_PostsUser()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Dashboards.ChangeOwnerAsync("~123456", new DashboardOwnerChangeRequest { User = "lucas@example.com" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/dashboard/~123456/owner");
		stub.Calls[0].Body.Should().Be("""{"user":"lucas@example.com"}""");
	}

	[Fact]
	public async Task ChangeOwnerAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Missing permission manageUser"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Dashboards.ChangeOwnerAsync("~123456", new DashboardOwnerChangeRequest { User = "x" }, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
