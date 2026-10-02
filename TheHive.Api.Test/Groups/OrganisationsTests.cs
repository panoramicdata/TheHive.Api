using System.Net;
using Refit;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Organisations;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class OrganisationsTests
{
	private const string OrganisationJson = """
		{
			"_id":"~128458762","_type":"Organisation","_createdBy":"admin@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"name":"TheOrganization",
			"description":"The primary investigation organization.","taskRule":"manual","observableRule":"autoShare",
			"links":[{"toOrganisation":"Partner","avatar":"iVBORw0KGgo=","linkType":"default","otherLinkType":"supervised"}],
			"avatar":"iVBORw0KGgoAAAANSUhEUg==","locked":true,"extraData":{"users":3}
		}
		""";

	private const string MinimalOrganisationJson = """
		{
			"_id":"~1","_type":"Organisation","_createdBy":"admin@example.com","_createdAt":1748739600000,
			"name":"o","description":"d","taskRule":"weird","observableRule":"manual","locked":false,"extraData":{}
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

	private static void AssertFullOrganisation(Organisation item)
	{
		item.Id.Should().Be("~128458762");
		item.Type.Should().Be("Organisation");
		item.CreatedBy.Should().Be("admin@example.com");
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		item.Name.Should().Be("TheOrganization");
		item.Description.Should().Be("The primary investigation organization.");
		item.TaskRule.Should().Be(SharingRule.Manual);
		item.ObservableRule.Should().Be(SharingRule.AutoShare);
		item.Links.Should().ContainSingle().Which.Should().Match<OrganisationLink>(
			l => l.ToOrganisation == "Partner" && l.Avatar == "iVBORw0KGgo=" && l.LinkType == "default" && l.OtherLinkType == "supervised");
		item.Avatar.Should().Be("iVBORw0KGgoAAAANSUhEUg==");
		item.Locked.Should().BeTrue();
		item.ExtraData["users"].GetInt32().Should().Be(3);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, OrganisationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.CreateAsync(
			new OrganisationCreateRequest
			{
				Name = "TheOrganization",
				Description = "The primary investigation organization.",
				TaskRule = SharingRule.Manual,
				ObservableRule = SharingRule.AutoShare,
				Locked = true
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"TheOrganization","description":"The primary investigation organization.","taskRule":"manual","observableRule":"autoShare","locked":true}""");
		AssertFullOrganisation(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalOrganisationJson);
		using var client = TestClient.Create(stub);

		await client.Organisations.CreateAsync(new OrganisationCreateRequest { Name = "o", Description = "d" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"o","description":"d"}""");
	}

	[Fact]
	public async Task GetAsync_FullOrganisation_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, OrganisationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.GetAsync("The Organization", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/The%20Organization");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullOrganisation(result);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults_And_UnknownRuleIsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalOrganisationJson);
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.GetAsync("~1", TestContext.Current.CancellationToken);

		result.TaskRule.Should().Be(SharingRule.Unknown);
		result.ObservableRule.Should().Be(SharingRule.Manual);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Avatar.Should().BeNull();
		result.Links.Should().BeEmpty();
		result.Locked.Should().BeFalse();
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var item = new Organisation();
		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.Links.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();

		var details = new OrganisationLinkDetails();
		details.LinkType.Should().BeEmpty();
		details.OtherLinkType.Should().BeEmpty();
		details.Organisation.Should().NotBeNull();

		var profile = new SharingProfile();
		profile.Name.Should().BeEmpty();
		profile.Description.Should().BeEmpty();
		profile.PermissionProfile.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Organisations.UpdateAsync(
			"~128458762",
			new OrganisationUpdateRequest
			{
				Name = "Renamed",
				Description = "New description",
				TaskRule = SharingRule.AutoShare,
				ObservableRule = SharingRule.Manual,
				Locked = false,
				Avatar = "iVBORw0KGgo="
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"Renamed","description":"New description","taskRule":"autoShare","observableRule":"manual","locked":false,"avatar":"iVBORw0KGgo="}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Organisations.UpdateAsync("~128458762", new OrganisationUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task GetAvatarAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] image = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];
		stub.EnqueueFile(image, "application/octet-stream", "avatar.png");
		using var client = TestClient.Create(stub);

		using var content = await client.Organisations.GetAvatarAsync("~128458762", "fake-avatar-hash", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762/avatar/fake-avatar-hash");
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

		var act = () => client.Organisations.GetAvatarAsync("~128458762", "fake-avatar-hash", "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task LinkAsync_PutsProfiles()
	{
		var stub = Stub(HttpStatusCode.Created);
		using var client = TestClient.Create(stub);

		await client.Organisations.LinkAsync(
			"~128458762",
			"Partner Org",
			new OrganisationLinkRequest { LinkType = "default", OtherLinkType = "supervised" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762/link/Partner%20Org");
		stub.Calls[0].Body.Should().Be("""{"linkType":"default","otherLinkType":"supervised"}""");
	}

	[Fact]
	public async Task LinkAsync_EmptyRequest_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.Created);
		using var client = TestClient.Create(stub);

		await client.Organisations.LinkAsync("~1", "~2", new OrganisationLinkRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task UnlinkAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Organisations.UnlinkAsync("~128458762", "~354", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762/link/~354");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ListLinksAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $$"""[{"linkType":"default","otherLinkType":"supervised","organisation":{{OrganisationJson}}}]""");
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.ListLinksAsync("~128458762", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762/links");
		stub.Calls[0].Body.Should().BeNull();
		var link = result.Should().ContainSingle().Subject;
		link.LinkType.Should().Be("default");
		link.OtherLinkType.Should().Be("supervised");
		AssertFullOrganisation(link.Organisation);
	}

	[Fact]
	public async Task ReplaceLinksAsync_PutsLinks()
	{
		var stub = Stub(HttpStatusCode.Created);
		using var client = TestClient.Create(stub);

		await client.Organisations.ReplaceLinksAsync(
			"~128458762",
			new OrganisationBulkLinkRequest
			{
				Links =
				[
					new OrganisationLink { ToOrganisation = "Partner", LinkType = "default", OtherLinkType = "supervised" },
					new OrganisationLink { ToOrganisation = "Other", Avatar = "iVBORw0KGgo=", LinkType = "default", OtherLinkType = "default" }
				]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/organisation/~128458762/links");
		stub.Calls[0].Body.Should().Be(
			"""{"links":[{"toOrganisation":"Partner","linkType":"default","otherLinkType":"supervised"},{"toOrganisation":"Other","avatar":"iVBORw0KGgo=","linkType":"default","otherLinkType":"default"}]}""");
	}

	[Fact]
	public async Task GetAsync_LinkMissingRequiredLinkType_StillDeserializes()
	{
		var json = MinimalOrganisationJson.Replace(
			"\"locked\"",
			"\"links\":[{\"toOrganisation\":\"Partner\",\"otherLinkType\":\"supervised\"}],\"locked\"",
			StringComparison.Ordinal);
		var stub = Stub(HttpStatusCode.OK, json);
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.GetAsync("~1", TestContext.Current.CancellationToken);

		var link = result.Links.Should().ContainSingle().Subject;
		link.ToOrganisation.Should().Be("Partner");
		link.LinkType.Should().BeNull();
		link.OtherLinkType.Should().Be("supervised");
	}

	[Fact]
	public async Task ReplaceLinksAsync_EmptyAndNull_AreDistinguished()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created);
		stub.Enqueue(HttpStatusCode.Created);
		using var client = TestClient.Create(stub);

		await client.Organisations.ReplaceLinksAsync("~1", new OrganisationBulkLinkRequest { Links = [] }, TestContext.Current.CancellationToken);
		await client.Organisations.ReplaceLinksAsync("~1", new OrganisationBulkLinkRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"links":[]}""");
		stub.Calls[1].Body.Should().Be("{}");
	}

	[Fact]
	public async Task ListSharingProfilesAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, """
			[{"name":"default","description":"Default sharing profile.","autoShare":true,"editable":false,"permissionProfile":"analyst","taskRule":"autoShare","observableRule":"manual"}]
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Organisations.ListSharingProfilesAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/sharingProfile");
		stub.Calls[0].Body.Should().BeNull();
		var profile = result.Should().ContainSingle().Subject;
		profile.Name.Should().Be("default");
		profile.Description.Should().Be("Default sharing profile.");
		profile.AutoShare.Should().BeTrue();
		profile.Editable.Should().BeFalse();
		profile.PermissionProfile.Should().Be("analyst");
		profile.TaskRule.Should().Be(SharingRule.AutoShare);
		profile.ObservableRule.Should().Be(SharingRule.Manual);
	}

	[Fact]
	public async Task UploadAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart_And_ReturnsAttachments()
	{
		var stub = Stub(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);

		var result = await client.Organisations.UploadAttachmentsAsync(
			[new StreamPart(first, "sample.exe", "application/octet-stream"), new ByteArrayPart([4, 5], "notes.txt", "text/plain")],
			cancellationToken: TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/attachment");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "sample.exe" && p.ContentType == "application/octet-stream");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "notes.txt" && p.ContentType == "text/plain");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		var attachment = result.Attachments.Should().ContainSingle().Subject;
		attachment.Id.Should().Be("~456789012");
		attachment.StorageId.Should().Be("fake-storage-id");
		attachment.Name.Should().Be("sample.exe");
		attachment.ContentType.Should().Be("application/octet-stream");
		attachment.Size.Should().Be(4);
	}

	[Fact]
	public async Task UploadAttachmentsAsync_CanRename_IsSentAsATextPart()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Organisations.UploadAttachmentsAsync([new ByteArrayPart([1], "a.bin", "application/octet-stream")], true, TestContext.Current.CancellationToken);

		var parts = stub.Calls[0].Parts;
		parts.Should().HaveCount(2);
		parts.Should().ContainSingle(p => p.Name == "canRename").Which.Text.Should().Be("true");
		stub.Calls[0].Parts.Should().NotContain(p => p.Name == "canRename" && p.FileName != null);
	}

	[Fact]
	public async Task UploadAttachmentsAsync_OmitsCanRenameWhenNull()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Organisations.UploadAttachmentsAsync([new ByteArrayPart([1], "a.bin", "application/octet-stream")], cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Parts.Should().NotContain(p => p.Name == "canRename");
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Organisations.DeleteAttachmentAsync("~456789012", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/attachment/~456789012");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAttachmentAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] file = [0x4D, 0x5A, 0x00, 0xFF];
		stub.EnqueueFile(file, "application/octet-stream", "sample.exe");
		using var client = TestClient.Create(stub);

		using var content = await client.Organisations.GetAttachmentAsync("~456789012", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/attachment/~456789012");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Headers.Contains("If-None-Match").Should().BeFalse();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(file);
	}

	[Fact]
	public async Task GetAttachmentAsync_NotModified_SendsIfNoneMatchAndThrows()
	{
		var stub = Stub(HttpStatusCode.NotModified);
		using var client = TestClient.Create(stub);

		var act = () => client.Organisations.GetAttachmentAsync("~456789012", "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task DownloadAttachmentAsync_ReturnsExactBytesAndFileName()
	{
		var stub = new StubHandler();
		byte[] file = [0x00, 0x01, 0xFE, 0xFF];
		stub.EnqueueFile(file, "application/octet-stream", "sample.exe");
		using var client = TestClient.Create(stub);

		using var content = await client.Organisations.DownloadAttachmentAsync("~456789012", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/attachment/~456789012/download");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("sample.exe");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(file);
	}

	[Fact]
	public async Task DownloadAttachmentAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Attachment not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Organisations.DownloadAttachmentAsync("~0", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}

	[Fact]
	public async Task GetAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Organisations.GetAsync("TheOrganization", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
