using System.Net;
using TheHive.Api.Data.Comments;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CommentsTests
{
	private const string FullCommentJson = """
		{
			"_id":"~843456789","_type":"Comment","createdBy":"lucas@example.com","createdAt":1747214591000,
			"updatedAt":1747218191000,"updatedBy":"emma@example.com",
			"message":"Confirmed as ransomware. Containment measures initiated.","isEdited":true,
			"extraData":{"links":1},"external":true
		}
		""";

	private const string MinimalCommentJson = """
		{
			"_id":"~1","_type":"Comment","createdBy":"lucas@example.com","createdAt":1747214591000,
			"message":"m","isEdited":false,"extraData":{},"external":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task AddToCaseAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullCommentJson);
		using var client = TestClient.Create(stub);

		var result = await client.Comments.AddToCaseAsync(
			"~123",
			new CommentRequest { Message = "Confirmed as ransomware.", External = true },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/comment");
		stub.Calls[0].Body.Should().Be("""{"message":"Confirmed as ransomware.","external":true}""");
		result.Id.Should().Be("~843456789");
		result.Type.Should().Be("Comment");
		result.CreatedBy.Should().Be("lucas@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1747214591000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1747218191000));
		result.UpdatedBy.Should().Be("emma@example.com");
		result.Message.Should().Be("Confirmed as ransomware. Containment measures initiated.");
		result.IsEdited.Should().BeTrue();
		result.ExtraData["links"].GetInt32().Should().Be(1);
		result.External.Should().BeTrue();
	}

	[Fact]
	public async Task AddToCaseAsync_MessageOnly_OmitsExternal_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalCommentJson);
		using var client = TestClient.Create(stub);

		var result = await client.Comments.AddToCaseAsync("7", new CommentRequest { Message = "m" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/7/comment");
		stub.Calls[0].Body.Should().Be("""{"message":"m"}""");
		result.UpdatedAt.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.IsEdited.Should().BeFalse();
		result.External.Should().BeFalse();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void Comment_Defaults_AreEmptyNotNull()
	{
		var item = new Comment();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Message.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task AddToAlertAsync_PostsBodyAndMapsComment()
	{
		var stub = Stub(HttpStatusCode.Created, FullCommentJson);
		using var client = TestClient.Create(stub);

		var result = await client.Comments.AddToAlertAsync("~354", new CommentRequest { Message = "Triaged" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/comment");
		stub.Calls[0].Body.Should().Be("""{"message":"Triaged"}""");
		result.Id.Should().Be("~843456789");
		result.Message.Should().Be("Confirmed as ransomware. Containment measures initiated.");
	}

	[Fact]
	public async Task UpdateAsync_PatchesMessageAndExternal()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Comments.UpdateAsync(
			"~843456789",
			new CommentRequest { Message = "Edited", External = false },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/comment/~843456789");
		stub.Calls[0].Body.Should().Be("""{"message":"Edited","external":false}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Comments.DeleteAsync("~843456789", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/comment/~843456789");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Comments.DeleteAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e =>
				e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError" && e.Message == "Not allowed");
	}
}
