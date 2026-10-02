using System.Net;
using TheHive.Api.Data.Pages;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class PagesTests
{
	private const string PageJson = """
		{
			"_id":"~84123456","_type":"Page","_createdBy":"sami@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1718532000000,"_updatedAt":1718618400000,"title":"Investigation Notes",
			"content":"## Summary\n\nFile encryption via PowerShell script detected on multiple workstations.",
			"order":2,"category":"Investigation","extraData":{"usage":3}
		}
		""";

	private const string MinimalPageJson = """
		{
			"_id":"~1","_type":"Page","_createdBy":"sami@example.com","_createdAt":1718532000000,
			"title":"t","content":"c","order":0,"category":"Investigation","extraData":{}
		}
		""";

	private const string CreateBodyJson = """{"title":"Investigation Notes","content":"## Summary","order":2,"category":"Investigation"}""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static PageCreateRequest CreateRequest() => new()
	{
		Title = "Investigation Notes",
		Content = "## Summary",
		Order = 2,
		Category = "Investigation"
	};

	private static void AssertFullPage(Page item)
	{
		item.Id.Should().Be("~84123456");
		item.Type.Should().Be("Page");
		item.CreatedBy.Should().Be("sami@example.com");
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718618400000));
		item.Title.Should().Be("Investigation Notes");
		item.Content.Should().Be("## Summary\n\nFile encryption via PowerShell script detected on multiple workstations.");
		item.Order.Should().Be(2);
		item.Category.Should().Be("Investigation");
		item.ExtraData["usage"].GetInt32().Should().Be(3);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, PageJson);
		using var client = TestClient.Create(stub);

		var result = await client.Pages.CreateAsync(CreateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/page");
		stub.Calls[0].Body.Should().Be(CreateBodyJson);
		AssertFullPage(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOrder_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalPageJson);
		using var client = TestClient.Create(stub);

		var result = await client.Pages.CreateAsync(
			new PageCreateRequest { Title = "t", Content = "c", Category = "Investigation" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"title":"t","content":"c","category":"Investigation"}""");
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void Page_Defaults_AreEmptyNotNull()
	{
		var item = new Page();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Content.Should().BeEmpty();
		item.Category.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Pages.UpdateAsync(
			"~84123456",
			new PageUpdateRequest { Title = "New title", Content = "New content", Order = 1, Category = "Lessons learned" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/page/~84123456");
		stub.Calls[0].Body.Should().Be("""{"title":"New title","content":"New content","order":1,"category":"Lessons learned"}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Pages.UpdateAsync("~84123456", new PageUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Pages.DeleteAsync("~84123456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/page/~84123456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task CreateInCaseAsync_PostsBodyAndMapsResult()
	{
		var stub = Stub(HttpStatusCode.Created, PageJson);
		using var client = TestClient.Create(stub);

		var result = await client.Pages.CreateInCaseAsync("~123", CreateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/page");
		stub.Calls[0].Body.Should().Be(CreateBodyJson);
		AssertFullPage(result);
	}

	[Fact]
	public async Task UpdateInCaseAsync_PatchesBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Pages.UpdateInCaseAsync("~123", "~84123456", new PageUpdateRequest { Title = "New title", Order = 0 }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/page/~84123456");
		stub.Calls[0].Body.Should().Be("""{"title":"New title","order":0}""");
	}

	[Fact]
	public async Task DeleteInCaseAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Pages.DeleteInCaseAsync("~123", "~84123456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/page/~84123456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Page ~1 not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Pages.DeleteAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
