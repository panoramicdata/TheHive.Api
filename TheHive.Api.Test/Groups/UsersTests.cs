using System.Net;
using Refit;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Organisations;
using TheHive.Api.Data.Users;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class UsersTests
{
	private const string UserJson = """
		{
			"_id":"~192024","_createdBy":"admin@example.com","_updatedBy":"root@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"login":"lucas@example.com","name":"Sami Analyst",
			"email":"notify@example.com","hasKey":true,"hasPassword":true,"hasMFA":true,"locked":false,"profile":"analyst",
			"permissions":["manageCase","manageAlert"],"organisation":"TheOrganization",
			"avatar":"api/v1/user/~1048576/avatar/fake-avatar-hash",
			"organisations":[{"organisationId":"~128458762","organisation":"TheOrganization","profile":"analyst",
				"avatar":"api/v1/organisation/~1048576/avatar/fake-avatar-hash",
				"links":[{"toOrganisation":"Partner","linkType":"default","otherLinkType":"supervised"}]}],
			"type":"Normal","defaultOrganisation":"TheOrganization","extraData":{"x":1}
		}
		""";

	private const string MinimalUserJson = """
		{
			"_id":"~1","_createdBy":"admin@example.com","_createdAt":1748739600000,"login":"a@example.com","name":"A",
			"hasKey":false,"hasPassword":false,"hasMFA":false,"locked":true,"profile":"read-only","organisation":"o",
			"type":"Robot","extraData":{}
		}
		""";

	private const string AttachmentJson = """
		{
			"_id":"~456789012","_type":"Attachment","_createdBy":"lucas@example.com","_createdAt":1748739600000,
			"name":"sample.exe","hashes":["fake-hash-0001"],"size":4,"contentType":"application/octet-stream",
			"id":"fake-storage-id","path":"attachments/fake-storage-id","extraData":{},"external":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullUser(User item)
	{
		item.Id.Should().Be("~192024");
		item.CreatedBy.Should().Be("admin@example.com");
		item.UpdatedBy.Should().Be("root@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		item.Login.Should().Be("lucas@example.com");
		item.Name.Should().Be("Sami Analyst");
		item.Email.Should().Be("notify@example.com");
		item.HasKey.Should().BeTrue();
		item.HasPassword.Should().BeTrue();
		item.HasMfa.Should().BeTrue();
		item.Locked.Should().BeFalse();
		item.Profile.Should().Be("analyst");
		item.Permissions.Should().Equal("manageCase", "manageAlert");
		item.Organisation.Should().Be("TheOrganization");
		item.Avatar.Should().Be("api/v1/user/~1048576/avatar/fake-avatar-hash");
		var membership = item.Organisations.Should().ContainSingle().Subject;
		membership.OrganisationId.Should().Be("~128458762");
		membership.Organisation.Should().Be("TheOrganization");
		membership.Profile.Should().Be("analyst");
		membership.Avatar.Should().Be("api/v1/organisation/~1048576/avatar/fake-avatar-hash");
		membership.Links.Should().ContainSingle().Which.Should().Match<OrganisationLink>(
			l => l.ToOrganisation == "Partner" && l.LinkType == "default" && l.OtherLinkType == "supervised");
		item.Type.Should().Be(UserType.Normal);
		item.DefaultOrganisation.Should().Be("TheOrganization");
		item.ExtraData["x"].GetInt32().Should().Be(1);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, UserJson);
		using var client = TestClient.Create(stub);

		var result = await client.Users.CreateAsync(
			new UserCreateRequest
			{
				Login = "sami@example.com",
				Name = "Sami Analyst",
				Email = "sami@example.com",
				Password = "fake-password",
				Profile = "analyst",
				Organisation = "TheOrganization",
				Type = UserType.Normal
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be(
			"""{"login":"sami@example.com","name":"Sami Analyst","email":"sami@example.com","password":"fake-password","profile":"analyst","organisation":"TheOrganization","type":"Normal"}""");
		AssertFullUser(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalUserJson);
		using var client = TestClient.Create(stub);

		await client.Users.CreateAsync(
			new UserCreateRequest { Login = "a@example.com", Name = "A", Profile = "read-only" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"login":"a@example.com","name":"A","profile":"read-only"}""");
	}

	[Fact]
	public async Task GetAsync_FullUser_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, UserJson);
		using var client = TestClient.Create(stub);

		var result = await client.Users.GetAsync("lucas@example.com", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/lucas%40example.com");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullUser(result);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults_And_UnknownTypeIsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalUserJson);
		using var client = TestClient.Create(stub);

		var result = await client.Users.GetAsync("~1", TestContext.Current.CancellationToken);

		result.Type.Should().Be(UserType.Unknown);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Email.Should().BeNull();
		result.Avatar.Should().BeNull();
		result.DefaultOrganisation.Should().BeNull();
		result.Permissions.Should().BeEmpty();
		result.Organisations.Should().BeEmpty();
		result.Locked.Should().BeTrue();
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var item = new User();
		item.Id.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Login.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Profile.Should().BeEmpty();
		item.Organisation.Should().BeEmpty();
		item.Permissions.Should().BeEmpty();
		item.Organisations.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();

		var membership = new UserOrganisationProfile();
		membership.OrganisationId.Should().BeEmpty();
		membership.Organisation.Should().BeEmpty();
		membership.Profile.Should().BeEmpty();
		membership.Links.Should().BeEmpty();

		var set = new UserOrganisation();
		set.Organisation.Should().BeEmpty();
		set.Profile.Should().BeEmpty();
		new UserOrganisationsResult().Organisations.Should().BeEmpty();
	}

	[Fact]
	public async Task GetCurrentAsync_Gets()
	{
		var stub = Stub(HttpStatusCode.OK, UserJson);
		using var client = TestClient.Create(stub);

		var result = await client.Users.GetCurrentAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/current");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullUser(result);
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.UpdateAsync(
			"~192024",
			new UserUpdateRequest
			{
				Name = "Emma Analyst",
				Organisation = "TheOrganization",
				Profile = "analyst",
				Locked = false,
				Avatar = "iVBORw0KGgo=",
				Email = "emma@example.com",
				DefaultOrganisation = "TheOrganization",
				Type = UserType.Service
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"Emma Analyst","organisation":"TheOrganization","profile":"analyst","locked":false,"avatar":"iVBORw0KGgo=","email":"emma@example.com","defaultOrganisation":"TheOrganization","type":"Service"}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.UpdateAsync("~192024", new UserUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task UpdateAsync_NullAvatarAndEmail_SendExplicitNulls()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.UpdateAsync(
			"~192024",
			new UserUpdateRequest { Avatar = default(string?), Email = default(string?) },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"avatar":null,"email":null}""");
	}

	[Fact]
	public async Task GetAvatarAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] image = [0xFF, 0xD8, 0xFF, 0x00];
		stub.EnqueueFile(image, "application/octet-stream", "avatar.jpg");
		using var client = TestClient.Create(stub);

		using var content = await client.Users.GetAvatarAsync("~192024", "fake-avatar-hash", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/avatar/fake-avatar-hash");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Headers.Contains("If-None-Match").Should().BeFalse();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(image);
	}

	[Fact]
	public async Task GetAvatarAsync_NotModified_SendsIfNoneMatchAndThrows()
	{
		var stub = Stub(HttpStatusCode.NotModified);
		using var client = TestClient.Create(stub);

		var act = () => client.Users.GetAvatarAsync("~192024", "fake-avatar-hash", "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task DeleteAsync_WithOrganisation_SendsItAsQuery()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.DeleteAsync("~192024", "The Org", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/force");
		stub.Calls[0].Uri.Query.Should().Be("?organisation=The%20Org");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_WithoutOrganisation_OmitsQuery()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.DeleteAsync("~192024", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/force");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task SetOrganisationsAsync_PutsMembershipsAndMapsResult()
	{
		var stub = Stub(HttpStatusCode.OK, """{"organisations":[{"organisation":"TheOrganization","profile":"analyst","default":true}]}""");
		using var client = TestClient.Create(stub);

		var result = await client.Users.SetOrganisationsAsync(
			"~192024",
			new UserOrganisationsSetRequest
			{
				Organisations =
				[
					new UserOrganisationInput { Organisation = "TheOrganization", Profile = "analyst", Default = true },
					new UserOrganisationInput { Organisation = "Other", Profile = "read-only" }
				]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/organisations");
		stub.Calls[0].Body.Should().Be(
			"""{"organisations":[{"organisation":"TheOrganization","profile":"analyst","default":true},{"organisation":"Other","profile":"read-only"}]}""");
		result.Organisations.Should().ContainSingle().Which.Should().Match<UserOrganisation>(
			o => o.Organisation == "TheOrganization" && o.Profile == "analyst" && o.Default);
	}

	[Fact]
	public async Task SetOrganisationsAsync_NoList_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		var result = await client.Users.SetOrganisationsAsync("~1", new UserOrganisationsSetRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
		result.Organisations.Should().BeEmpty();
	}

	[Fact]
	public async Task SetPasswordAsync_PostsPasswordInBodyOnly()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.SetPasswordAsync("~192024", new UserPasswordSetRequest { Password = "fake-password" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/password/set");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"password":"fake-password"}""");
	}

	[Fact]
	public async Task ChangePasswordAsync_PostsBothPasswordsInBodyOnly()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.ChangePasswordAsync(
			"lucas@example.com",
			new UserPasswordChangeRequest { Password = "fake-new-password", CurrentPassword = "fake-password" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/lucas%40example.com/password/change");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"password":"fake-new-password","currentPassword":"fake-password"}""");
	}

	[Fact]
	public async Task GetApiKeyAsync_ReturnsPlainTextKey()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "fake-api-key", r => r.Content = new StringContent("fake-api-key", System.Text.Encoding.UTF8, "text/plain"));
		using var client = TestClient.Create(stub);

		var key = await client.Users.GetApiKeyAsync("~192024", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/key");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
		key.Should().Be("fake-api-key");
	}

	[Fact]
	public async Task RevokeApiKeyAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.RevokeApiKeyAsync("~192024", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/key");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task RenewApiKeyAsync_PostsAndReturnsPlainTextKey()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "unused", r => r.Content = new StringContent("NewKey0123456789", System.Text.Encoding.UTF8, "text/plain"));
		using var client = TestClient.Create(stub);

		var key = await client.Users.RenewApiKeyAsync("~192024", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/user/~192024/key/renew");
		stub.Calls[0].Body.Should().BeNull();
		key.Should().Be("NewKey0123456789");
	}

	[Fact]
	public async Task UploadTemporaryAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart_And_ReturnsAttachments()
	{
		var stub = Stub(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);

		var result = await client.Users.UploadTemporaryAttachmentsAsync(
			[new StreamPart(first, "sample.exe", "application/octet-stream"), new ByteArrayPart([4, 5], "notes.txt", "text/plain")],
			cancellationToken: TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/user/current/attachments");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "sample.exe" && p.ContentType == "application/octet-stream");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "notes.txt" && p.ContentType == "text/plain");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		var attachment = result.Attachments.Should().ContainSingle().Subject;
		attachment.Id.Should().Be("~456789012");
		attachment.Name.Should().Be("sample.exe");
		attachment.Size.Should().Be(4);
	}

	[Fact]
	public async Task UploadTemporaryAttachmentsAsync_CanRename_IsSentAsATextPart()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Users.UploadTemporaryAttachmentsAsync([new ByteArrayPart([1], "a.bin", "application/octet-stream")], true, TestContext.Current.CancellationToken);

		var parts = stub.Calls[0].Parts;
		parts.Should().HaveCount(2);
		parts.Should().ContainSingle(p => p.Name == "canRename").Which.Text.Should().Be("true");
	}

	[Fact]
	public async Task UploadTemporaryAttachmentsAsync_OmitsCanRenameWhenNull()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Users.UploadTemporaryAttachmentsAsync([new ByteArrayPart([1], "a.bin", "application/octet-stream")], cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Parts.Should().NotContain(p => p.Name == "canRename");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"User not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Users.GetAsync("nobody@example.com", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}

	[Fact]
	public async Task SetPasswordAsync_Forbidden_ThrowsWithoutEchoingThePassword()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Users.SetPasswordAsync("~1", new UserPasswordSetRequest { Password = "fake-password" }, TestContext.Current.CancellationToken);

		var thrown = (await act.Should().ThrowAsync<TheHiveApiException>()).Which;
		thrown.StatusCode.Should().Be(HttpStatusCode.Forbidden);
		thrown.Message.Should().NotContain("fake-password");
	}
}
