using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Licenses;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class LicenseTests
{
	private const string FullLicenseJson = """
		{
			"_id":"~84123","_createdAt":1736849400000,"_createdBy":"lucas@example.com","_type":"License","id":"LIC-GOLD-2025-001",
			"customer":"TheOrganization","plan":"Gold","kind":"Regular","validFrom":1719792000000,"expiresAt":1814400000000,
			"current":true,"capabilities":["auth.basic","auth.ldap","case.customStatus"],
			"quotas":{"organisations":5,"users.normal":20,"case.template":-1}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullLicense(License item)
	{
		item.Id.Should().Be("~84123");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1736849400000));
		item.CreatedBy.Should().Be("lucas@example.com");
		item.Type.Should().Be("License");
		item.LicenseId.Should().Be("LIC-GOLD-2025-001");
		item.Customer.Should().Be("TheOrganization");
		item.Plan.Should().Be("Gold");
		item.Kind.Should().Be("Regular");
		item.ValidFrom.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1719792000000));
		item.ExpiresAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1814400000000));
		item.Current.Should().BeTrue();
		item.Capabilities.Should().Equal("auth.basic", "auth.ldap", "case.customStatus");
		item.Quotas.Should().BeEquivalentTo(new Dictionary<string, int> { ["organisations"] = 5, ["users.normal"] = 20, ["case.template"] = -1 });
	}

	[Fact]
	public async Task ListAsync_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{FullLicenseJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.License.ListAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullLicense(result.Should().ContainSingle().Which);
	}

	[Fact]
	public async Task ListAsync_EmptyObjectNil_IsAnEmptyList()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		var result = await client.License.ListAsync(TestContext.Current.CancellationToken);

		result.Should().BeEmpty();
	}

	[Theory]
	[InlineData("{}")]
	[InlineData("{ \n }")]
	[InlineData("null")]
	[InlineData("[]")]
	public void LicenseList_EmptyForms_ReadAsAnEmptyList(string json)
	{
		var result = JsonSerializer.Deserialize<LicenseList>(json, TheHiveJson.Options);

		result.Should().NotBeNull().And.BeEmpty();
	}

	[Theory]
	[InlineData("""{"unexpected":1}""")]
	[InlineData("""{"licenses":[]}""")]
	public void LicenseList_NonEmptyObject_ThrowsJsonExceptionWithAClearMessage(string json)
	{
		var act = () => JsonSerializer.Deserialize<LicenseList>(json, TheHiveJson.Options);

		act.Should().Throw<JsonException>().WithMessage("*license array or an empty object*");
	}

	[Fact]
	public void LicenseList_Serializes_AsAnArray()
	{
		var list = new LicenseList { new License { Id = "~1" } };

		var json = JsonSerializer.Serialize(list, TheHiveJson.Options);

		json.Should().StartWith("""[{"_id":"~1",""").And.EndWith("]");
		JsonSerializer.Deserialize<LicenseList>(json, TheHiveJson.Options)!.Should().ContainSingle().Which.Id.Should().Be("~1");
	}

	[Fact]
	public async Task GetAsync_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullLicenseJson);
		using var client = TestClient.Create(stub);

		var result = await client.License.GetAsync("LIC-GOLD-2025-001", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license/LIC-GOLD-2025-001");
		AssertFullLicense(result);
	}

	[Fact]
	public void License_Defaults_AreEmptyNotNull()
	{
		var item = new License();

		item.Id.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.LicenseId.Should().BeEmpty();
		item.Customer.Should().BeEmpty();
		item.Plan.Should().BeEmpty();
		item.Kind.Should().BeEmpty();
		item.Capabilities.Should().BeEmpty();
		item.Quotas.Should().BeEmpty();
	}

	[Fact]
	public async Task AddAsync_PostsTheKeyInTheBody_And204IsNull()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		var result = await client.License.AddAsync(new LicenseAddRequest { License = "fake-license-key" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"license":"fake-license-key"}""");
		result.Should().BeNull();
	}

	[Fact]
	public async Task AddAsync_ActivationErrorWith200_IsReturnedAsRawJson()
	{
		var stub = Stub(HttpStatusCode.OK, """{"error":"License has expired"}""");
		using var client = TestClient.Create(stub);

		var result = await client.License.AddAsync(new LicenseAddRequest { License = "fake-license-key" }, TestContext.Current.CancellationToken);

		result!.Value.GetProperty("error").GetString().Should().Be("License has expired");
	}

	[Fact]
	public async Task ActivateAsync_PutsWithoutBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.License.ActivateAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license/~84123/activate");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetChallengeAsync_ReturnsPlainText()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, "fake-challenge-token", r => r.Content = new StringContent("fake-challenge-token", System.Text.Encoding.UTF8, "text/plain"));
		using var client = TestClient.Create(stub);

		var result = await client.License.GetChallengeAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license/challenge");
		result.Should().Be("fake-challenge-token");
	}

	[Fact]
	public async Task GetCurrentAsync_ValidLicense_MapsLicenseOnly()
	{
		var stub = Stub(HttpStatusCode.OK, $$"""{"license":{{FullLicenseJson}}}""");
		using var client = TestClient.Create(stub);

		var result = await client.License.GetCurrentAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/license/current");
		AssertFullLicense(result.License!);
		result.Fallback.Should().BeNull();
		result.Error.Should().BeNull();
		result.NotFound.Should().BeNull();
	}

	[Fact]
	public async Task GetCurrentAsync_FailedValidation_MapsErrorAndFallback()
	{
		var stub = Stub(HttpStatusCode.OK, $$"""{"error":"License has expired","fallback":{{FullLicenseJson}}}""");
		using var client = TestClient.Create(stub);

		var result = await client.License.GetCurrentAsync(TestContext.Current.CancellationToken);

		result.Error.Should().Be("License has expired");
		AssertFullLicense(result.Fallback!);
		result.License.Should().BeNull();
	}

	[Fact]
	public async Task GetCurrentAsync_NoLicense_MapsNotFoundAndFallback()
	{
		var stub = Stub(HttpStatusCode.OK, $$"""{"fallback":{{FullLicenseJson}},"notFound":true}""");
		using var client = TestClient.Create(stub);

		var result = await client.License.GetCurrentAsync(TestContext.Current.CancellationToken);

		result.NotFound.Should().BeTrue();
		AssertFullLicense(result.Fallback!);
	}

	[Fact]
	public void LicenseStatus_And_Quota_MapEveryField()
	{
		const string json = """
			{
				"id":"lic-a1b2c3d4","customer":"TheOrganization","instance":"thehive-prod-01","plan":"Platinum","kind":"Regular",
				"validFrom":1735689600000,"expiresAt":1798761600000,"capabilities":["auth.sso","case.timeline"],"isValid":false,
				"error":"License signature verification failed.",
				"quotas":{"users.normal":{"current":8,"quota":25},"organisations":{"quota":-1}}
			}
			""";

		var item = JsonSerializer.Deserialize<LicenseStatus>(json, TheHiveJson.Options)!;

		item.Id.Should().Be("lic-a1b2c3d4");
		item.Customer.Should().Be("TheOrganization");
		item.Instance.Should().Be("thehive-prod-01");
		item.Plan.Should().Be("Platinum");
		item.Kind.Should().Be("Regular");
		item.ValidFrom.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1735689600000));
		item.ExpiresAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1798761600000));
		item.Capabilities.Should().Equal("auth.sso", "case.timeline");
		item.IsValid.Should().BeFalse();
		item.Error.Should().Be("License signature verification failed.");
		item.Quotas["users.normal"].Current.Should().Be(8);
		item.Quotas["users.normal"].Quota.Should().Be(25);
		item.Quotas["organisations"].Current.Should().BeNull();
		item.Quotas["organisations"].Quota.Should().Be(-1);
	}

	[Fact]
	public void LicenseStatus_Defaults_AreEmptyNotNull()
	{
		var item = new LicenseStatus();

		item.Id.Should().BeEmpty();
		item.Customer.Should().BeEmpty();
		item.Instance.Should().BeEmpty();
		item.Plan.Should().BeEmpty();
		item.Kind.Should().BeEmpty();
		item.Capabilities.Should().BeEmpty();
		item.Error.Should().BeNull();
		item.Quotas.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.License.GetAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
