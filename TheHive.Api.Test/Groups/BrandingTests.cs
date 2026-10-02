using System.Net;
using Refit;
using TheHive.Api.Data.Branding;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class BrandingTests
{
	private const string FullBrandingJson = """
		{
			"title":"TheOrganization","loginLogo":"api/v1/branding/assets/loginLogo",
			"menuLogo":"api/v1/branding/assets/menuLogo","favicon":"api/v1/branding/assets/favicon"
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullBranding(BrandingSettings item)
	{
		item.Title.Should().Be("TheOrganization");
		item.LoginLogo.Should().Be("api/v1/branding/assets/loginLogo");
		item.MenuLogo.Should().Be("api/v1/branding/assets/menuLogo");
		item.Favicon.Should().Be("api/v1/branding/assets/favicon");
	}

	[Fact]
	public async Task GetAsync_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullBrandingJson);
		using var client = TestClient.Create(stub);

		var result = await client.Branding.GetAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/branding");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullBranding(result);
	}

	[Fact]
	public async Task GetAsync_NothingConfigured_MapsNulls()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		var result = await client.Branding.GetAsync(TestContext.Current.CancellationToken);

		result.Title.Should().BeNull();
		result.LoginLogo.Should().BeNull();
		result.MenuLogo.Should().BeNull();
		result.Favicon.Should().BeNull();
	}

	[Fact]
	public async Task SetAsync_UploadsTitleAndEveryImageAsParts()
	{
		var stub = Stub(HttpStatusCode.OK, FullBrandingJson);
		using var client = TestClient.Create(stub);
		byte[] login = [0x89, 0x50, 0x4E, 0x47, 0x01];
		byte[] menu = [0xFF, 0xD8, 0xFF, 0x02];
		byte[] icon = [0x89, 0x50, 0x4E, 0x47, 0x03];

		var result = await client.Branding.SetAsync(
			"TheOrganization",
			new ByteArrayPart(login, "login.png", "image/png"),
			new ByteArrayPart(menu, "menu.jpg", "image/jpeg"),
			new ByteArrayPart(icon, "favicon.png", "image/png"),
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/branding");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(4);
		call.Parts[0].Name.Should().Be("title");
		call.Parts[0].Text.Should().Be("TheOrganization");
		call.Parts[1].Name.Should().Be("loginLogo");
		call.Parts[1].FileName.Should().Be("login.png");
		call.Parts[1].ContentType.Should().Be("image/png");
		call.Parts[1].Bytes.Should().Equal(login);
		call.Parts[2].Name.Should().Be("menuLogo");
		call.Parts[2].FileName.Should().Be("menu.jpg");
		call.Parts[2].ContentType.Should().Be("image/jpeg");
		call.Parts[2].Bytes.Should().Equal(menu);
		call.Parts[3].Name.Should().Be("favicon");
		call.Parts[3].FileName.Should().Be("favicon.png");
		call.Parts[3].Bytes.Should().Equal(icon);
		AssertFullBranding(result);
	}

	[Fact]
	public async Task SetAsync_TitleOnly_LeavesOutTheNullParts()
	{
		var stub = Stub(HttpStatusCode.OK, FullBrandingJson);
		using var client = TestClient.Create(stub);

		await client.Branding.SetAsync("New title", cancellationToken: TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("title");
		part.Text.Should().Be("New title");
	}

	[Fact]
	public async Task SetAsync_FaviconOnly_UploadsJustThatPart()
	{
		var stub = Stub(HttpStatusCode.OK, FullBrandingJson);
		using var client = TestClient.Create(stub);

		await client.Branding.SetAsync(favicon: new ByteArrayPart([1, 2, 3], "f.png", "image/png"), cancellationToken: TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("favicon");
		part.Bytes.Should().Equal(1, 2, 3);
	}

	[Fact]
	public async Task SetAsync_NonSeekableStream_IsUploadedOnce()
	{
		var stub = Stub(HttpStatusCode.OK, FullBrandingJson);
		using var client = TestClient.Create(stub);
		using var stream = new NonSeekableStream([7, 8, 9]);

		await client.Branding.SetAsync(loginLogo: new StreamPart(stream, "l.png", "image/png"), cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Parts.Should().ContainSingle().Which.Bytes.Should().Equal(7, 8, 9);
	}

	[Fact]
	public async Task DeleteAssetAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Branding.DeleteAssetAsync(BrandingAssetKinds.LoginLogo, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/branding/assets/loginLogo");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Theory]
	[InlineData(BrandingAssetKinds.LoginLogo, "loginLogo")]
	[InlineData(BrandingAssetKinds.MenuLogo, "menuLogo")]
	[InlineData(BrandingAssetKinds.Favicon, "favicon")]
	public async Task GetAssetAsync_And_DeleteAssetAsync_WriteEachSpecKindToThePathVerbatim(string kind, string wire)
	{
		var stub = new StubHandler();
		stub.EnqueueFile([1, 2], "application/octet-stream", "asset.bin");
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		using var content = await client.Branding.GetAssetAsync(kind, cancellationToken: TestContext.Current.CancellationToken);
		await client.Branding.DeleteAssetAsync(kind, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be($"/api/v1/branding/assets/{wire}");
		stub.Calls[1].Uri.AbsolutePath.Should().Be($"/api/v1/branding/assets/{wire}");
	}

	[Fact]
	public async Task GetAssetAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] image = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];
		stub.EnqueueFile(image, "application/octet-stream", "menuLogo.png");
		using var client = TestClient.Create(stub);

		using var content = await client.Branding.GetAssetAsync(BrandingAssetKinds.MenuLogo, cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/branding/assets/menuLogo");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Headers.Contains("If-None-Match").Should().BeFalse();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("menuLogo.png");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(image);
	}

	[Fact]
	public async Task GetAssetAsync_NotModified_SendsIfNoneMatchAndThrows()
	{
		var stub = Stub(HttpStatusCode.NotModified);
		using var client = TestClient.Create(stub);

		var act = () => client.Branding.GetAssetAsync(BrandingAssetKinds.Favicon, "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task SetAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Platinum licence required"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Branding.SetAsync("t", cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
