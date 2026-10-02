using System.Net;
using TheHive.Api.Data.Pages;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class PageTemplatesTests
{
	private const string PageTemplateJson = """
		{
			"_id":"~84123999","_type":"Page","_createdBy":"sami@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1718532000000,"_updatedAt":1718618400000,"title":"Incident report",
			"content":"## Timeline","order":1,"category":"Reports","extraData":{}
		}
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
		var stub = Stub(HttpStatusCode.Created, PageTemplateJson);
		using var client = TestClient.Create(stub);

		var result = await client.PageTemplates.CreateAsync(
			new PageCreateRequest { Title = "Incident report", Content = "## Timeline", Order = 1, Category = "Reports" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pageTemplate");
		stub.Calls[0].Body.Should().Be("""{"title":"Incident report","content":"## Timeline","order":1,"category":"Reports"}""");
		result.Id.Should().Be("~84123999");
		result.Type.Should().Be("Page");
		result.CreatedBy.Should().Be("sami@example.com");
		result.UpdatedBy.Should().Be("lucas@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718618400000));
		result.Title.Should().Be("Incident report");
		result.Content.Should().Be("## Timeline");
		result.Order.Should().Be(1);
		result.Category.Should().Be("Reports");
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.PageTemplates.UpdateAsync(
			"~84123999",
			new PageUpdateRequest { Title = "New title", Content = "New content", Order = 3, Category = "Lessons" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pageTemplate/~84123999");
		stub.Calls[0].Body.Should().Be("""{"title":"New title","content":"New content","order":3,"category":"Lessons"}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.PageTemplates.DeleteAsync("~84123999", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pageTemplate/~84123999");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Missing permission managePageTemplate"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.PageTemplates.UpdateAsync("~1", new PageUpdateRequest(), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
