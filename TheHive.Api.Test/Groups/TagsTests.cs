using System.Net;
using TheHive.Api.Data.Tags;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class TagsTests
{
	private const string TagJson = """
		{
			"_id":"~83456","_type":"Tag","_createdBy":"emma@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1744660800000,"_updatedAt":1744747200000,"namespace":"_freetags_","predicate":"ransomware",
			"value":"amber","description":"Identifies ransomware-related cases and observables","colour":"#e8560a",
			"hidden":true,"extraData":{"usage":4}
		}
		""";

	private const string MinimalTagJson = """
		{
			"_id":"~1","_type":"Tag","_createdBy":"emma@example.com","_createdAt":1744660800000,
			"namespace":"_freetags_","predicate":"p","colour":"#000000","hidden":false,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task GetAsync_FullTag_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, TagJson);
		using var client = TestClient.Create(stub);

		var item = await client.Tags.GetAsync("~83456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/tag/~83456");
		stub.Calls[0].Body.Should().BeNull();
		item.Id.Should().Be("~83456");
		item.Type.Should().Be("Tag");
		item.CreatedBy.Should().Be("emma@example.com");
		item.UpdatedBy.Should().Be("sami@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1744660800000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1744747200000));
		item.Namespace.Should().Be("_freetags_");
		item.Predicate.Should().Be("ransomware");
		item.Value.Should().Be("amber");
		item.Description.Should().Be("Identifies ransomware-related cases and observables");
		item.Colour.Should().Be("#e8560a");
		item.Hidden.Should().BeTrue();
		item.ExtraData["usage"].GetInt32().Should().Be(4);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalTagJson);
		using var client = TestClient.Create(stub);

		var item = await client.Tags.GetAsync("~1", TestContext.Current.CancellationToken);

		item.UpdatedBy.Should().BeNull();
		item.UpdatedAt.Should().BeNull();
		item.Value.Should().BeNull();
		item.Description.Should().BeNull();
		item.Hidden.Should().BeFalse();
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var item = new Tag();
		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Namespace.Should().BeEmpty();
		item.Predicate.Should().BeEmpty();
		item.Colour.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tags.UpdateAsync(
			"ransom ware",
			new TagUpdateRequest { Predicate = "ransomware", Description = "New description", Colour = "#e8560a" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/tag/ransom%20ware");
		stub.Calls[0].Body.Should().Be("""{"predicate":"ransomware","description":"New description","colour":"#e8560a"}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tags.UpdateAsync("~83456", new TagUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Tags.DeleteAsync("~83456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/tag/~83456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Tag not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Tags.GetAsync("~0", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
