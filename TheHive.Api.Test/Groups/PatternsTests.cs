using System.Net;
using Refit;
using TheHive.Api.Data.Patterns;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class PatternsTests
{
	private const string FullCatalogJson = """
		{
			"_id":"~81920","_type":"CatalogOfPattern","createdBy":"admin@example.com","createdAt":1722513600000,
			"updatedAt":1748995200000,"name":"Enterprise Attack","description":"Standard techniques","variant":"2025-04-15",
			"extraData":{"patternCount":678}
		}
		""";

	private const string MinimalCatalogJson = """
		{"_id":"~1","_type":"CatalogOfPattern","createdBy":"a@example.com","createdAt":1722513600000,"name":"c","extraData":{}}
		""";

	private const string FullPatternJson = """
		{
			"_id":"~163840","_type":"Pattern","_createdBy":"admin@example.com","_updatedBy":"emma@example.com",
			"_createdAt":1722513600000,"_updatedAt":1748995200000,"patternId":"T1486","name":"Data Encrypted for Impact",
			"description":"Adversaries may encrypt data.","tactics":["impact"],"url":"https://attack.mitre.org/techniques/T1486",
			"patternType":"attack-pattern","capecId":"CAPEC-242","capecUrl":"https://capec.mitre.org/data/definitions/242.html",
			"revoked":true,"dataSources":["Process","File"],"defenseBypassed":["Antivirus"],"detection":"Monitor process execution.",
			"permissionsRequired":["User"],"platforms":["Windows","macOS","Linux"],"remoteSupport":true,
			"systemRequirements":["Elevated privileges"],"version":"1.3","extraData":{"k":"v"}
		}
		""";

	private const string MinimalPatternJson = """
		{
			"_id":"~2","_type":"Pattern","_createdBy":"a@example.com","_createdAt":1722513600000,"patternId":"T1","name":"n",
			"url":"https://attack.mitre.org/techniques/T1","patternType":"attack-pattern","revoked":false,"remoteSupport":false,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullPattern(Pattern item)
	{
		item.Id.Should().Be("~163840");
		item.Type.Should().Be("Pattern");
		item.CreatedBy.Should().Be("admin@example.com");
		item.UpdatedBy.Should().Be("emma@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1722513600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748995200000));
		item.PatternId.Should().Be("T1486");
		item.Name.Should().Be("Data Encrypted for Impact");
		item.Description.Should().Be("Adversaries may encrypt data.");
		item.Tactics.Should().Equal("impact");
		item.Url.Should().Be("https://attack.mitre.org/techniques/T1486");
		item.PatternType.Should().Be("attack-pattern");
		item.CapecId.Should().Be("CAPEC-242");
		item.CapecUrl.Should().Be("https://capec.mitre.org/data/definitions/242.html");
		item.Revoked.Should().BeTrue();
		item.DataSources.Should().Equal("Process", "File");
		item.DefenseBypassed.Should().Equal("Antivirus");
		item.Detection.Should().Be("Monitor process execution.");
		item.PermissionsRequired.Should().Equal("User");
		item.Platforms.Should().Equal("Windows", "macOS", "Linux");
		item.RemoteSupport.Should().BeTrue();
		item.SystemRequirements.Should().Equal("Elevated privileges");
		item.Version.Should().Be("1.3");
		item.ExtraData["k"].GetString().Should().Be("v");
	}

	[Fact]
	public async Task CreateCatalogAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullCatalogJson);
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.CreateCatalogAsync(
			new PatternCatalogCreateRequest { Name = "Enterprise Attack", Description = "Standard techniques", Variant = "2025-04-15" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/catalog");
		stub.Calls[0].Body.Should().Be("""{"name":"Enterprise Attack","description":"Standard techniques","variant":"2025-04-15"}""");
		result.Id.Should().Be("~81920");
		result.Type.Should().Be("CatalogOfPattern");
		result.CreatedBy.Should().Be("admin@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1722513600000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748995200000));
		result.Name.Should().Be("Enterprise Attack");
		result.Description.Should().Be("Standard techniques");
		result.Variant.Should().Be("2025-04-15");
		result.ExtraData["patternCount"].GetInt32().Should().Be(678);
	}

	[Fact]
	public async Task CreateCatalogAsync_NameOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalCatalogJson);
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.CreateCatalogAsync(new PatternCatalogCreateRequest { Name = "c" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"c"}""");
		result.UpdatedAt.Should().BeNull();
		result.Description.Should().BeNull();
		result.Variant.Should().BeNull();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void Catalog_And_Pattern_Defaults_AreEmptyNotNull()
	{
		var catalog = new PatternCatalog();
		var pattern = new Pattern();

		catalog.Id.Should().BeEmpty();
		catalog.Type.Should().BeEmpty();
		catalog.CreatedBy.Should().BeEmpty();
		catalog.Name.Should().BeEmpty();
		catalog.ExtraData.Should().BeEmpty();
		pattern.Id.Should().BeEmpty();
		pattern.Type.Should().BeEmpty();
		pattern.CreatedBy.Should().BeEmpty();
		pattern.PatternId.Should().BeEmpty();
		pattern.Name.Should().BeEmpty();
		pattern.Url.Should().BeEmpty();
		pattern.PatternType.Should().BeEmpty();
		pattern.Tactics.Should().BeEmpty();
		pattern.DataSources.Should().BeEmpty();
		pattern.DefenseBypassed.Should().BeEmpty();
		pattern.PermissionsRequired.Should().BeEmpty();
		pattern.Platforms.Should().BeEmpty();
		pattern.SystemRequirements.Should().BeEmpty();
		pattern.ExtraData.Should().BeEmpty();
		new PatternImportResult().Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateCatalogAsync_PatchesNameAndExplicitNulls()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Patterns.UpdateCatalogAsync(
			"Enterprise Attack",
			new PatternCatalogUpdateRequest { Name = "Enterprise Attack v2", Description = null, Variant = null },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/catalog/Enterprise%20Attack");
		stub.Calls[0].Body.Should().Be("""{"name":"Enterprise Attack v2","description":null,"variant":null}""");
	}

	[Fact]
	public async Task UpdateCatalogAsync_SetsValues_And_EmptyRequestSendsEmptyObject()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Patterns.UpdateCatalogAsync("~81920", new PatternCatalogUpdateRequest { Description = "d", Variant = "v" }, TestContext.Current.CancellationToken);
		await client.Patterns.UpdateCatalogAsync("~81920", new PatternCatalogUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"description":"d","variant":"v"}""");
		stub.Calls[1].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteCatalogAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Patterns.DeleteCatalogAsync("~81920", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/catalog/~81920");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_MapsEveryPatternField()
	{
		var stub = Stub(HttpStatusCode.OK, FullPatternJson);
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.GetAsync("T1486", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pattern/T1486");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullPattern(result);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalPatternJson);
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.GetAsync("~2", TestContext.Current.CancellationToken);

		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Description.Should().BeNull();
		result.CapecId.Should().BeNull();
		result.CapecUrl.Should().BeNull();
		result.Detection.Should().BeNull();
		result.Version.Should().BeNull();
		result.Tactics.Should().BeEmpty();
		result.Platforms.Should().BeEmpty();
		result.Revoked.Should().BeFalse();
	}

	[Fact]
	public async Task ListForCaseAsync_MapsList()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{FullPatternJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.ListForCaseAsync("~354", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pattern/case/~354");
		AssertFullPattern(result.Should().ContainSingle().Which);
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Patterns.DeleteAsync("T1486", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pattern/T1486");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ImportAsync_PostsUrlBody_AndMaps201Result()
	{
		var stub = Stub(HttpStatusCode.Created, """{"success":678}""");
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.ImportAsync(
			new PatternImportRequest { Url = "https://example.test/enterprise-attack.json", Catalog = "Enterprise Attack", Variant = "2025-06-26" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/pattern/import/attack");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be("""{"url":"https://example.test/enterprise-attack.json","catalog":"Enterprise Attack","variant":"2025-06-26"}""");
		result.Success.Should().Be(678);
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task ImportAsync_PartialSuccess207_IsReturnedWithErrors()
	{
		var stub = Stub(HttpStatusCode.MultiStatus, """{"success":672,"errors":["Pattern T1099 is already in the catalog"]}""");
		using var client = TestClient.Create(stub);

		var result = await client.Patterns.ImportAsync(new PatternImportRequest { Catalog = "c" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"catalog":"c"}""");
		result.Success.Should().Be(672);
		result.Errors.Should().Equal("Pattern T1099 is already in the catalog");
	}

	[Fact]
	public async Task ImportFileAsync_UploadsJsonAndFileParts()
	{
		var stub = Stub(HttpStatusCode.Created, """{"success":3}""");
		using var client = TestClient.Create(stub);
		byte[] file = [0x7B, 0x7D, 0x0A];

		var result = await client.Patterns.ImportFileAsync(
			new PatternImportRequest { Catalog = "Enterprise Attack", Variant = "2025-06-26" },
			new ByteArrayPart(file, "enterprise-attack.json", "application/json"),
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/pattern/import/attack");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Name.Should().Be("_json");
		call.Parts[0].ContentType.Should().Be("application/json");
		call.Parts[0].Text.Should().Be("""{"catalog":"Enterprise Attack","variant":"2025-06-26"}""");
		call.Parts[1].Name.Should().Be("file");
		call.Parts[1].FileName.Should().Be("enterprise-attack.json");
		call.Parts[1].ContentType.Should().Be("application/json");
		call.Parts[1].Bytes.Should().Equal(file);
		result.Success.Should().Be(3);
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Pattern not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Patterns.GetAsync("T0000", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
