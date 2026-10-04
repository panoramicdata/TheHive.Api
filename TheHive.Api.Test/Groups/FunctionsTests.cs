using System.Net;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Functions;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class FunctionsTests
{
	private const string Code = "function handle(input, context) { return input; }";

	private const string FunctionJson = """
		{
			"_id":"~84123","_type":"Function","_createdBy":"emma@example.com","_createdAt":1748739600000,
			"_updatedBy":"sami@example.com","_updatedAt":1748739605000,"name":"my-function","mode":"Enabled",
			"definition":"function handle(input, context) { return input; }","description":"Echoes its input",
			"config":{"threshold":3},"lastSuccessDate":1748739610000,"lastSuccessDetails":{"alerts":2},
			"lastErrorDate":1748739620000,"lastErrorDetails":{"message":"boom"},"types":["api","feeder:alert"]
		}
		""";

	private const string MinimalFunctionJson = """
		{
			"_id":"~1","_type":"Function","_createdBy":"emma@example.com","_createdAt":1748739600000,
			"name":"f","mode":"Disabled","definition":"x","config":{}
		}
		""";

	private const string InvocationJson = """{"result":{"echo":1},"durationMillis":12,"stdout":"out","stderr":"err"}""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertInvocation(FunctionInvocationResult result)
	{
		result.Result!.Value.GetProperty("echo").GetInt32().Should().Be(1);
		result.DurationMillis.Should().Be(12);
		result.Stdout.Should().Be("out");
		result.Stderr.Should().Be("err");
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FunctionJson);
		using var client = TestClient.Create(stub);

		var function = await client.Functions.CreateAsync(
			new FunctionCreateRequest
			{
				Name = "my-function",
				Mode = FunctionMode.DryRun,
				Definition = Code,
				Description = "Echoes its input",
				Config = new() { ["threshold"] = 3, ["token"] = "fake-key" },
				Types = [FunctionType.Api, FunctionType.ActionCase, FunctionType.ActionAlert, FunctionType.FeederAlert, FunctionType.Notification]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"my-function","mode":"DryRun","definition":"function handle(input, context) { return input; }","description":"Echoes its input","config":{"threshold":3,"token":"fake-key"},"types":["api","action:case","action:alert","feeder:alert","notification"]}""");
		function.Id.Should().Be("~84123");
		function.Type.Should().Be("Function");
		function.CreatedBy.Should().Be("emma@example.com");
		function.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		function.UpdatedBy.Should().Be("sami@example.com");
		function.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739605000));
		function.Name.Should().Be("my-function");
		function.Mode.Should().Be("Enabled");
		function.Definition.Should().Be(Code);
		function.Description.Should().Be("Echoes its input");
		function.Config["threshold"].GetInt32().Should().Be(3);
		function.LastSuccessDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739610000));
		function.LastSuccessDetails!.Value.GetProperty("alerts").GetInt32().Should().Be(2);
		function.LastErrorDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739620000));
		function.LastErrorDetails!.Value.GetProperty("message").GetString().Should().Be("boom");
		function.Types.Should().Equal("api", "feeder:alert");
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals_AndMinimalResponseMapsToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalFunctionJson);
		using var client = TestClient.Create(stub);

		var function = await client.Functions.CreateAsync(
			new FunctionCreateRequest { Name = "f", Definition = "x" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"f","definition":"x"}""");
		function.UpdatedBy.Should().BeNull();
		function.UpdatedAt.Should().BeNull();
		function.Description.Should().BeNull();
		function.Config.Should().BeEmpty();
		function.LastSuccessDate.Should().BeNull();
		function.LastSuccessDetails.Should().BeNull();
		function.LastErrorDate.Should().BeNull();
		function.LastErrorDetails.Should().BeNull();
		function.Types.Should().BeEmpty();
	}

	[Fact]
	public async Task GetContextDocumentationAsync_Gets_AndMapsEveryField()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			"""{"items":[{"name":"alert","kind":"Field"},{"name":"create","kind":"Method","args":["alert"]},{"name":"odd","kind":"Mystery"}]}""");
		using var client = TestClient.Create(stub);

		var documentation = await client.Functions.GetContextDocumentationAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/_context/documentation");
		documentation.Items.Should().HaveCount(3);
		documentation.Items[0].Name.Should().Be("alert");
		documentation.Items[0].Kind.Should().Be(FunctionContextItemKind.Field);
		documentation.Items[0].Args.Should().BeNull();
		documentation.Items[1].Kind.Should().Be(FunctionContextItemKind.Method);
		documentation.Items[1].Args.Should().Equal("alert");
		documentation.Items[2].Kind.Should().Be(FunctionContextItemKind.Unknown);
	}

	[Fact]
	public async Task TestAsync_PostsBodyWithDryRun_AndMapsTheResult()
	{
		var stub = Stub(HttpStatusCode.OK, InvocationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Functions.TestAsync(
			new FunctionTestRequest
			{
				Name = "scratch",
				Definition = Code,
				Config = new() { ["token"] = "fake-key" },
				Input = new { source = "unit" }
			},
			new DryRunOptions { DryRun = true },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/_test");
		stub.Calls[0].Uri.Query.Should().Be("?dryRun=true");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"scratch","definition":"function handle(input, context) { return input; }","config":{"token":"fake-key"},"input":{"source":"unit"}}""");
		AssertInvocation(result);
	}

	[Fact]
	public async Task TestAsync_RequiredOnly_OmitsOptionalsAndQuery()
	{
		var stub = Stub(HttpStatusCode.OK, InvocationJson);
		using var client = TestClient.Create(stub);

		await client.Functions.TestAsync(new FunctionTestRequest { Definition = "x" }, new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"definition":"x"}""");
	}

	[Fact]
	public async Task InvokeAsync_PostsTheInputPayload_WithDryRunFalseLowercase()
	{
		var stub = Stub(HttpStatusCode.OK, InvocationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Functions.InvokeAsync("my function", new { source = "unit" }, new DryRunOptions { DryRun = false }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/my%20function");
		stub.Calls[0].Uri.Query.Should().Be("?dryRun=false");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be("""{"source":"unit"}""");
		AssertInvocation(result);
	}

	[Fact]
	public async Task InvokeAsync_WithoutInput_SendsTheJsonLiteralNull()
	{
		var stub = Stub(HttpStatusCode.OK, InvocationJson);
		using var client = TestClient.Create(stub);

		await client.Functions.InvokeAsync("~84123", null, new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/~84123");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("null");
	}

	[Fact]
	public async Task InvokeOnObjectAsync_PostsWithoutBody_AndSendsFlagsLowercase()
	{
		var stub = Stub(HttpStatusCode.OK, InvocationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Functions.InvokeOnObjectAsync("~84123", "alert", "type;source;ref", new FunctionInvocationOptions { DryRun = true, Sync = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/~84123/alert/type%3Bsource%3Bref");
		stub.Calls[0].Uri.Query.Should().Be("?dryRun=true&sync=true");
		stub.Calls[0].Body.Should().BeNull();
		AssertInvocation(result);
	}

	[Fact]
	public async Task InvokeOnObjectAsync_BackgroundRun_ReadsTheEmptyObjectAsDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		var result = await client.Functions.InvokeOnObjectAsync("f", "case", "7", new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/f/case/7");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		result.Result.Should().BeNull();
		result.DurationMillis.Should().Be(0);
		result.Stdout.Should().BeEmpty();
		result.Stderr.Should().BeEmpty();
	}

	[Fact]
	public async Task InvokeOnObjectAsync_OptionsObject_SendsMixedFlagsLowercase()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		await client.Functions.InvokeOnObjectAsync("f", "case", "7", new FunctionInvocationOptions { DryRun = true, Sync = false }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?dryRun=true&sync=false");
	}

	[Fact]
	public async Task InvokeOnObjectAsync_PartialOptions_LeavesOutTheNullFlag()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		await client.Functions.InvokeOnObjectAsync("f", "case", "7", new FunctionInvocationOptions { Sync = false }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?sync=false");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Functions.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_Gets()
	{
		var stub = Stub(HttpStatusCode.OK, FunctionJson);
		using var client = TestClient.Create(stub);

		var function = await client.Functions.GetAsync("my-function", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/my-function");
		stub.Calls[0].Body.Should().BeNull();
		function.Id.Should().Be("~84123");
	}

	[Fact]
	public async Task UpdateAsync_PatchesOnlyTheSetFields()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Functions.UpdateAsync(
			"~84123",
			new FunctionUpdateRequest
			{
				Mode = FunctionMode.Disabled,
				Definition = Code,
				Description = "d",
				Config = new() { ["token"] = "fake-key" },
				Types = [FunctionType.Api]
			},
			TestContext.Current.CancellationToken);
		await client.Functions.UpdateAsync("~84123", new FunctionUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/function/~84123");
		stub.Calls[0].Body.Should().Be(
			"""{"mode":"Disabled","definition":"function handle(input, context) { return input; }","description":"d","config":{"token":"fake-key"},"types":["api"]}""");
		stub.Calls[1].Body.Should().Be("{}");
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var function = new Function();
		function.Id.Should().BeEmpty();
		function.Type.Should().BeEmpty();
		function.CreatedBy.Should().BeEmpty();
		function.Name.Should().BeEmpty();
		function.Mode.Should().BeEmpty();
		function.Definition.Should().BeEmpty();
		function.Config.Should().BeEmpty();
		function.Types.Should().BeEmpty();

		new FunctionInvocationResult().Stdout.Should().BeEmpty();
		new FunctionContextItem().Name.Should().BeEmpty();
		new FunctionContextDocumentation().Items.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Function not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Functions.GetAsync("missing", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
