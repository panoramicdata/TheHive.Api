using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Views;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ViewsTests
{
	private const string FilterJson = """{"_and":[{"_eq":{"_field":"status","_value":"InProgress"}}]}""";

	private const string ListOptionsJson = """{"itemsPerPage":50,"listAsGroup":false,"autoRefresh":true,"statsIsOpen":true}""";

	private const string ViewJson = $$"""
		{
			"_id":"~344112","_type":"ListView","_createdAt":1748476800000,"_createdBy":"alice@example.com",
			"_updatedAt":1748563200000,"_updatedBy":"lucas@example.com","name":"High severity cases","entity":"Case",
			"filter":{{FilterJson}},"listOptions":{{ListOptionsJson}},"sortList":["-_createdAt"],"showColumns":["tlp","tags"],"isShared":true
		}
		""";

	private const string MinimalViewJson = $$"""
		{
			"_id":"~1","_type":"ListView","_createdAt":1748476800000,"_createdBy":"alice@example.com",
			"name":"n","entity":"Case","filter":{},"listOptions":{{ListOptionsJson}},"isShared":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static JsonElement Filter() => JsonSerializer.Deserialize<JsonElement>(FilterJson);

	private static ViewListOptions Options() => new() { ItemsPerPage = 50, ListAsGroup = false, AutoRefresh = true, StatsIsOpen = true };

	private static void AssertFullView(View item)
	{
		item.Id.Should().Be("~344112");
		item.Type.Should().Be("ListView");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748476800000));
		item.CreatedBy.Should().Be("alice@example.com");
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748563200000));
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.Name.Should().Be("High severity cases");
		item.Entity.Should().Be(ViewEntity.Case);
		item.Filter.GetRawText().Should().Be(FilterJson);
		item.ListOptions.ItemsPerPage.Should().Be(50);
		item.ListOptions.ListAsGroup.Should().BeFalse();
		item.ListOptions.AutoRefresh.Should().BeTrue();
		item.ListOptions.StatsIsOpen.Should().BeTrue();
		item.SortList.Should().Equal("-_createdAt");
		item.ShowColumns.Should().Equal("tlp", "tags");
		item.IsShared.Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, ViewJson);
		using var client = TestClient.Create(stub);

		var result = await client.Views.CreateAsync(
			new ViewCreateRequest
			{
				Name = "High severity cases",
				Entity = ViewEntity.Case,
				Filter = Filter(),
				ListOptions = Options(),
				SortList = ["-_createdAt"],
				ShowColumns = ["tlp", "tags"],
				IsShared = true
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/views");
		stub.Calls[0].Body.Should().Be(
			$$"""{"name":"High severity cases","entity":"Case","filter":{{FilterJson}},"listOptions":{{ListOptionsJson}},"sortList":["-_createdAt"],"showColumns":["tlp","tags"],"isShared":true}""");
		AssertFullView(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalViewJson);
		using var client = TestClient.Create(stub);

		var result = await client.Views.CreateAsync(
			new ViewCreateRequest
			{
				Name = "n",
				Entity = ViewEntity.AttackPatterns,
				Filter = JsonSerializer.Deserialize<JsonElement>("{}"),
				ListOptions = Options(),
				IsShared = false
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			$$"""{"name":"n","entity":"AttackPatterns","filter":{},"listOptions":{{ListOptionsJson}},"isShared":false}""");
		result.UpdatedAt.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.SortList.Should().BeEmpty();
		result.ShowColumns.Should().BeEmpty();
		result.IsShared.Should().BeFalse();
	}

	[Fact]
	public async Task GetAsync_UnknownEntity_IsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalViewJson.Replace("\"Case\"", "\"Hologram\"", StringComparison.Ordinal));
		using var client = TestClient.Create(stub);

		var result = await client.Views.GetAsync("344112", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/views/344112");
		stub.Calls[0].Body.Should().BeNull();
		result.Entity.Should().Be(ViewEntity.Unknown);
	}

	[Fact]
	public async Task GetAsync_FullView_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, ViewJson);
		using var client = TestClient.Create(stub);

		var result = await client.Views.GetAsync("~344112", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/views/~344112");
		AssertFullView(result);
	}

	[Fact]
	public void View_Defaults_AreEmptyNotNull()
	{
		var item = new View();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Filter.ValueKind.Should().Be(JsonValueKind.Undefined);
		item.ListOptions.ItemsPerPage.Should().Be(50);
		item.SortList.Should().BeEmpty();
		item.ShowColumns.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Views.UpdateAsync(
			"~344112",
			new ViewUpdateRequest
			{
				Name = "New name",
				Filter = Filter(),
				ListOptions = Options(),
				SortList = ["+_createdAt"],
				ShowColumns = ["pap"],
				IsShared = false
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/views/~344112");
		// System.Text.Json writes "+" as the JSON escape backslash-u002B, which is equivalent JSON; undo it to compare.
		var escapedPlus = ((char)92) + "u002B";
		stub.Calls[0].Body!.Replace(escapedPlus, "+", StringComparison.Ordinal).Should().Be(
			$$"""{"name":"New name","filter":{{FilterJson}},"listOptions":{{ListOptionsJson}},"sortList":["+_createdAt"],"showColumns":["pap"],"isShared":false}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Views.UpdateAsync("~344112", new ViewUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Views.DeleteAsync("~344112", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/views/~344112");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"ListView ~1 not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Views.GetAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
