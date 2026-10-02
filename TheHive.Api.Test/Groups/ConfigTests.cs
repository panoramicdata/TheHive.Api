using System.Net;
using System.Text.Json;
using TheHive.Api.Data.UserConfig;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ConfigTests
{
	private const string ItemJson = """
		{"path":"dashboards","defaultValue":[],"value":{"~7741591688":{"period":"Last7Days","refresh":30}}}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task ListAsync_WithoutPath_ReturnsTheWholeConfiguration()
	{
		var stub = Stub(HttpStatusCode.OK, """{"organisation":"TheOrganization","profile":"analyst","list-views-cases":[],"notification":{"items":[]}}""");
		using var client = TestClient.Create(stub);

		var result = await client.Config.ListAsync(cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/config/user");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
		result.GetProperty("organisation").GetString().Should().Be("TheOrganization");
		result.GetProperty("profile").GetString().Should().Be("analyst");
		result.GetProperty("notification").GetProperty("items").GetArrayLength().Should().Be(0);
	}

	[Fact]
	public async Task ListAsync_WithPath_SendsItAsTheQueryParameter()
	{
		var stub = Stub(HttpStatusCode.OK, """{"profile":"analyst"}""");
		using var client = TestClient.Create(stub);

		await client.Config.ListAsync("profile", TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/config/user?path=profile");
	}

	[Fact]
	public async Task GetAsync_MapsPathDefaultAndValue()
	{
		var stub = Stub(HttpStatusCode.OK, ItemJson);
		using var client = TestClient.Create(stub);

		var result = await client.Config.GetAsync("notification.items", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/config/user/notification.items");
		result.Path.Should().Be("dashboards");
		result.DefaultValue.ValueKind.Should().Be(JsonValueKind.Array);
		result.Value.GetProperty("~7741591688").GetProperty("refresh").GetInt32().Should().Be(30);
	}

	[Fact]
	public void UserConfigItem_Defaults_AreEmptyAndUndefined()
	{
		var item = new UserConfigItem();

		item.Path.Should().BeEmpty();
		item.DefaultValue.ValueKind.Should().Be(JsonValueKind.Undefined);
		item.Value.ValueKind.Should().Be(JsonValueKind.Undefined);
	}

	[Fact]
	public async Task SetAsync_PutsTheValueObject_AndMapsTheItem()
	{
		var stub = Stub(HttpStatusCode.OK, ItemJson);
		using var client = TestClient.Create(stub);
		var value = JsonSerializer.SerializeToElement(new { period = "Last7Days", refresh = 30 });

		var result = await client.Config.SetAsync("dashboards", new UserConfigSetRequest { Value = value }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/config/user/dashboards");
		stub.Calls[0].Body.Should().Be("""{"value":{"period":"Last7Days","refresh":30}}""");
		result.Path.Should().Be("dashboards");
	}

	[Theory]
	[InlineData("\"text\"")]
	[InlineData("true")]
	[InlineData("12")]
	[InlineData("[1,2]")]
	[InlineData("null")]
	public async Task SetAsync_AcceptsAnyJsonValue(string json)
	{
		var stub = Stub(HttpStatusCode.OK, ItemJson);
		using var client = TestClient.Create(stub);
		using var document = JsonDocument.Parse(json);

		await client.Config.SetAsync("k", new UserConfigSetRequest { Value = document.RootElement }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"value":{{json}}}""");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Unknown item"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Config.GetAsync("nope", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
