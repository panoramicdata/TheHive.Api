using System.Net;
using TheHive.Api.Data.ObservableTypes;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ObservableTypesTests
{
	private const string FullTypeJson = """
		{
			"_id":"~4096","_type":"ObservableType","_updatedAt":1718102400000,"_updatedBy":"lucas@example.com",
			"_createdAt":1718016000000,"_createdBy":"emma@example.com","name":"ja4-fingerprint",
			"isAttachment":true,"isCaseSensitive":true
		}
		""";

	private const string MinimalTypeJson = """
		{
			"_id":"~1","_type":"ObservableType","_createdAt":1718016000000,"_createdBy":"emma@example.com",
			"name":"ip","isAttachment":false,"isCaseSensitive":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullType(ObservableType item)
	{
		item.Id.Should().Be("~4096");
		item.Type.Should().Be("ObservableType");
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718102400000));
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718016000000));
		item.CreatedBy.Should().Be("emma@example.com");
		item.Name.Should().Be("ja4-fingerprint");
		item.IsAttachment.Should().BeTrue();
		item.IsCaseSensitive.Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullTypeJson);
		using var client = TestClient.Create(stub);

		var result = await client.ObservableTypes.CreateAsync(
			new ObservableTypeCreateRequest { Name = "ja4-fingerprint", IsAttachment = true, IsCaseSensitive = true },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/type");
		stub.Calls[0].Body.Should().Be("""{"name":"ja4-fingerprint","isAttachment":true,"isCaseSensitive":true}""");
		AssertFullType(result);
	}

	[Fact]
	public async Task CreateAsync_NameOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalTypeJson);
		using var client = TestClient.Create(stub);

		await client.ObservableTypes.CreateAsync(new ObservableTypeCreateRequest { Name = "ip" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"ip"}""");
	}

	[Fact]
	public async Task GetAsync_GetsByIdOrName_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalTypeJson);
		using var client = TestClient.Create(stub);

		var result = await client.ObservableTypes.GetAsync("my type", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/type/my%20type");
		stub.Calls[0].Body.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.IsAttachment.Should().BeFalse();
		result.IsCaseSensitive.Should().BeFalse();
	}

	[Fact]
	public async Task GetAsync_FullType_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullTypeJson);
		using var client = TestClient.Create(stub);

		AssertFullType(await client.ObservableTypes.GetAsync("~4096", TestContext.Current.CancellationToken));
	}

	[Fact]
	public void ObservableType_Defaults_AreEmptyNotNull()
	{
		var item = new ObservableType();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesCaseSensitivity()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.ObservableTypes.UpdateAsync("~4096", new ObservableTypeUpdateRequest { IsCaseSensitive = false }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/type/~4096");
		stub.Calls[0].Body.Should().Be("""{"isCaseSensitive":false}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.ObservableTypes.UpdateAsync("~4096", new ObservableTypeUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.ObservableTypes.DeleteAsync("~4096", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/type/~4096");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_InUse_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Observable type is in use"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.ObservableTypes.DeleteAsync("ip", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
