using System.Net;
using System.Text.Json.Nodes;
using TheHive.Api.Data.Query;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

/// <summary>Credential-free: a refused write throws before any request reaches the stub.</summary>
public class ReadOnlyOptionTests
{
	[Fact]
	public void IsReadOnly_DefaultsToFalse()
		=> new TheHiveClientOptions().IsReadOnly.Should().BeFalse();

	[Fact]
	public async Task Write_IsRefused_WhenIsReadOnly()
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		var act = () => client.Cases.DeleteAsync("~1", TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
		stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task Write_IsAllowed_WhenNotReadOnly()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteAsync("~1", TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task Get_IsAllowed_WhenIsReadOnly()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """{"_id":"~1"}""");
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		await client.Cases.GetAsync("~1", TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Get);
	}

	[Fact]
	public async Task QueryPost_IsAllowed_WhenIsReadOnly()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);
		var request = new QueryRequest { Query = [new JsonObject { ["_name"] = "listCase" }] };

		await client.Query.RunAsync(request, new QueryRunOptions(), TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle().Which.Method.Should().Be(HttpMethod.Post);
	}

	[Fact]
	public async Task OtherPost_IsRefused_WhenIsReadOnly()
	{
		var stub = new StubHandler();
		using var client = TestClient.Create(stub, o => o.IsReadOnly = true);

		var act = () => client.Cases.CreateAsync(new TheHive.Api.Data.Cases.CaseCreateRequest { Title = "t", Description = "d" }, TestContext.Current.CancellationToken);

		await act.Should().ThrowExactlyAsync<InvalidOperationException>();
		stub.Calls.Should().BeEmpty();
	}

	[Fact]
	public async Task RequestWithoutUri_IsRefused_WhenIsReadOnly()
	{
		var stub = new StubHandler();
		using var invoker = new HttpMessageInvoker(new TheHive.Api.Handlers.ReadOnlyHandler(new TheHiveClientOptions { IsReadOnly = true }) { InnerHandler = stub });
		using var request = new HttpRequestMessage(HttpMethod.Post, (Uri?)null);

		var act = () => invoker.SendAsync(request, TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<InvalidOperationException>();
		stub.Calls.Should().BeEmpty();
	}
}
