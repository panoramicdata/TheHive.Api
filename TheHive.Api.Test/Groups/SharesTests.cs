using System.Net;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Shares;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class SharesTests
{
	private const string ShareJson = """
		{
			"_id":"~1234567890","_type":"Share","_createdBy":"alice@example.com","_updatedBy":"bob@example.com",
			"_createdAt":1716299974000,"_updatedAt":1716299999000,"caseId":"~4123456789","profileName":"analyst",
			"organisationName":"TheOrganization","owner":true,"taskRule":"manual","observableRule":"autoShare"
		}
		""";

	private const string MinimalShareJson = """
		{
			"_id":"~1","_type":"Share","_createdBy":"a@example.com","_createdAt":1716299974000,"caseId":"~2",
			"profileName":"read-only","organisationName":"o","owner":false,"taskRule":"weird","observableRule":"manual"
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullShare(Share item)
	{
		item.Id.Should().Be("~1234567890");
		item.Type.Should().Be("Share");
		item.CreatedBy.Should().Be("alice@example.com");
		item.UpdatedBy.Should().Be("bob@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1716299974000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1716299999000));
		item.CaseId.Should().Be("~4123456789");
		item.ProfileName.Should().Be("analyst");
		item.OrganisationName.Should().Be("TheOrganization");
		item.Owner.Should().BeTrue();
		item.TaskRule.Should().Be(SharingRule.Manual);
		item.ObservableRule.Should().Be(SharingRule.AutoShare);
	}

	private const string TwoSharesBody = """
		{"shares":[{"organisation":"TheOrganization","share":true,"profile":"analyst","taskRule":"autoShare","observableRule":"autoShare"},{"organisation":"Other"}]}
		""";

	private static ShareCreateRequest TwoShares() => new()
	{
		Shares =
		[
			new ShareSettings
			{
				Organisation = "TheOrganization",
				Share = true,
				Profile = "analyst",
				TaskRule = SharingRule.AutoShare,
				ObservableRule = SharingRule.AutoShare
			},
			new ShareSettings { Organisation = "Other" }
		]
	};

	[Fact]
	public async Task ListByCaseAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.ListByCaseAsync("~354", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/shares");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullShare(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task ListByCaseAsync_AbsentOptionals_MapToDefaults_And_UnknownRuleIsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{MinimalShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.ListByCaseAsync("~354", TestContext.Current.CancellationToken);

		var share = result.Should().ContainSingle().Subject;
		share.UpdatedBy.Should().BeNull();
		share.UpdatedAt.Should().BeNull();
		share.Owner.Should().BeFalse();
		share.TaskRule.Should().Be(SharingRule.Unknown);
		share.ObservableRule.Should().Be(SharingRule.Manual);
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var item = new Share();
		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.CaseId.Should().BeEmpty();
		item.ProfileName.Should().BeEmpty();
		item.OrganisationName.Should().BeEmpty();
	}

	[Fact]
	public async Task ShareCaseAsync_PostsSharesAndMapsResult()
	{
		var stub = Stub(HttpStatusCode.Created, $"[{ShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.ShareCaseAsync("My Case", TwoShares(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/My%20Case/shares");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be(TwoSharesBody);
		AssertFullShare(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task SetCaseSharesAsync_PutsSharesAndMapsResult()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.SetCaseSharesAsync("~354", TwoShares(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/shares");
		stub.Calls[0].Body.Should().Be(TwoSharesBody);
		AssertFullShare(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task SetCaseSharesAsync_EmptyListAndNull_AreDistinguished()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "[]");
		stub.Enqueue(HttpStatusCode.OK, "[]");
		using var client = TestClient.Create(stub);

		await client.Shares.SetCaseSharesAsync("~1", new ShareCreateRequest { Shares = [] }, TestContext.Current.CancellationToken);
		await client.Shares.SetCaseSharesAsync("~1", new ShareCreateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"shares":[]}""");
		stub.Calls[1].Body.Should().Be("{}");
	}

	[Fact]
	public async Task UnshareCaseAsync_SendsDeleteWithBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.UnshareCaseAsync(
			"~354",
			new ShareRemoveRequest { Organisations = ["TheOrganization1", "TheOrganization2"] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/shares");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be("""{"organisations":["TheOrganization1","TheOrganization2"]}""");
	}

	[Fact]
	public async Task UnshareCaseAsync_NoOrganisations_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.UnshareCaseAsync("~354", new ShareRemoveRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.DeleteAsync("~1234567890", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/share/~1234567890");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAsync_PatchesProfile()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.UpdateAsync("~1234567890", new ShareUpdateRequest { Profile = "read-only" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/share/~1234567890");
		stub.Calls[0].Body.Should().Be("""{"profile":"read-only"}""");
	}

	[Fact]
	public async Task DeleteManyAsync_SendsDeleteWithIds()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.DeleteManyAsync(new ShareBulkDeleteRequest { Ids = ["~128458762", "~216513541"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/shares");
		stub.Calls[0].Body.Should().Be("""{"ids":["~128458762","~216513541"]}""");
	}

	[Fact]
	public async Task ListByTaskAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.ListByTaskAsync("~777", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~777/shares");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullShare(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task ShareTaskAsync_PostsOrganisations()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.ShareTaskAsync(
			"~777",
			new ShareOrganisationsRequest { Organisations = ["TheOrganization1", "TheOrganization2"] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~777/shares");
		stub.Calls[0].Body.Should().Be("""{"organisations":["TheOrganization1","TheOrganization2"]}""");
	}

	[Fact]
	public async Task ShareTaskAsync_NoOrganisations_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.ShareTaskAsync("~777", new ShareOrganisationsRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task UnshareTaskAsync_SendsDeleteWithBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.UnshareTaskAsync("~777", new ShareRemoveRequest { Organisations = ["Partner"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~777/shares");
		stub.Calls[0].Body.Should().Be("""{"organisations":["Partner"]}""");
	}

	[Fact]
	public async Task ListByObservableAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{ShareJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Shares.ListByObservableAsync("~888", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~888/shares");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullShare(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task ShareObservableAsync_PostsOrganisations()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.ShareObservableAsync("~888", new ShareOrganisationsRequest { Organisations = ["Partner"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~888/shares");
		stub.Calls[0].Body.Should().Be("""{"organisations":["Partner"]}""");
	}

	[Fact]
	public async Task UnshareObservableAsync_SendsDeleteWithBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Shares.UnshareObservableAsync("~888", new ShareRemoveRequest { Organisations = ["Partner"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~888/shares");
		stub.Calls[0].Body.Should().Be("""{"organisations":["Partner"]}""");
	}

	[Fact]
	public async Task ShareCaseAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Shares.ShareCaseAsync("~354", TwoShares(), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
