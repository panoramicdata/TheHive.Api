using System.Net;
using TheHive.Api.Data.Status;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class StatusTests
{
	private const string FullStatusJson = """
		{
			"version":"5.8.0","gitDescription":"a1b2c3d4e",
			"connectors":{"cortex":{"enabled":true,"status":"OK"},"misp":{"enabled":false,"status":"DISABLED"}},
			"config":{"protectDownloadsWith":"infected","authType":"local","multifactor":"disabled","pollingDuration":5000},
			"license":{
				"id":"lic-a1b2c3d4","customer":"TheOrganization","instance":"thehive-prod-01","plan":"Platinum","kind":"Regular",
				"validFrom":1735689600000,"expiresAt":1798761600000,"capabilities":["auth.sso"],"isValid":true,
				"quotas":{"users.normal":{"current":8,"quota":25}}
			},
			"cluster":{"leader":"pekko://thehive@10.0.0.1:2551","members":[{"address":"pekko://thehive@10.0.0.1:2551","status":"Up"}]},
			"schemaStatus":[{"name":"thehiveCore","currentVersion":42,"expectedVersion":42}],
			"features":["StreamSSE","Automation"]
		}
		""";

	private const string MinimalStatusJson = """
		{"version":"5.8.0","gitDescription":"a1b2c3d4e","connectors":{},"license":{"id":"l","isValid":false,"quotas":{}}}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task GetAsync_Default_SendsNoQuery_AndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullStatusJson);
		using var client = TestClient.Create(stub);

		var result = await client.Status.GetAsync(new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/status");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
		result.Version.Should().Be("5.8.0");
		result.GitDescription.Should().Be("a1b2c3d4e");
		result.Connectors.Should().HaveCount(2);
		result.Connectors["cortex"].GetProperty("status").GetString().Should().Be("OK");
		result.Connectors["misp"].GetProperty("enabled").GetBoolean().Should().BeFalse();
		result.Config!.Value.GetProperty("authType").GetString().Should().Be("local");
		result.License.Id.Should().Be("lic-a1b2c3d4");
		result.License.IsValid.Should().BeTrue();
		result.License.Quotas["users.normal"].Quota.Should().Be(25);
		result.Cluster!.Value.GetProperty("leader").GetString().Should().Be("pekko://thehive@10.0.0.1:2551");
		result.SchemaStatus.Should().ContainSingle().Which.GetProperty("currentVersion").GetInt32().Should().Be(42);
		result.Features.Should().Equal("StreamSSE", "Automation");
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalStatusJson);
		using var client = TestClient.Create(stub);

		var result = await client.Status.GetAsync(new(), TestContext.Current.CancellationToken);

		result.Config.Should().BeNull();
		result.Cluster.Should().BeNull();
		result.SchemaStatus.Should().BeEmpty();
		result.Features.Should().BeEmpty();
		result.Connectors.Should().BeEmpty();
	}

	[Theory]
	[InlineData(true, "?verbose=true")]
	[InlineData(false, "?verbose=false")]
	public async Task GetAsync_Verbose_IsSentLowercase(bool verbose, string query)
	{
		var stub = Stub(HttpStatusCode.OK, MinimalStatusJson);
		using var client = TestClient.Create(stub);

		await client.Status.GetAsync(new PlatformStatusQuery { Verbose = verbose }, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/status" + query);
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var status = new PlatformStatus();
		var publicStatus = new PublicStatus();
		var provider = new SsoProvider();

		status.Version.Should().BeEmpty();
		status.GitDescription.Should().BeEmpty();
		status.Connectors.Should().BeEmpty();
		status.License.Should().NotBeNull();
		publicStatus.Version.Should().BeEmpty();
		publicStatus.SsoProviders.Should().BeEmpty();
		publicStatus.Imports.MitreCatalog.Should().BeEmpty();
		provider.Name.Should().BeEmpty();
		provider.DisplayName.Should().BeEmpty();
		provider.Url.Should().BeEmpty();
	}

	[Fact]
	public async Task GetPublicAsync_MapsEveryField()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			"""
			{
				"sso":true,
				"ssoProviders":[{"name":"okta","displayName":"Sign in with Okta","url":"https://sso.example.com/oauth2/authorize"}],
				"version":"5.8.0","imports":{"mitreCatalog":"Complete"}
			}
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Status.GetPublicAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/status/public");
		stub.Calls[0].Body.Should().BeNull();
		result.Sso.Should().BeTrue();
		var provider = result.SsoProviders.Should().ContainSingle().Subject;
		provider.Name.Should().Be("okta");
		provider.DisplayName.Should().Be("Sign in with Okta");
		provider.Url.Should().Be("https://sso.example.com/oauth2/authorize");
		result.Version.Should().Be("5.8.0");
		result.Imports.MitreCatalog.Should().Be("Complete");
	}

	[Fact]
	public async Task GetPublicAsync_WithoutProviders_MapsEmptyList()
	{
		var stub = Stub(HttpStatusCode.OK, """{"sso":false,"version":"5.8.0","imports":{"mitreCatalog":"Idle"}}""");
		using var client = TestClient.Create(stub);

		var result = await client.Status.GetPublicAsync(TestContext.Current.CancellationToken);

		result.Sso.Should().BeFalse();
		result.SsoProviders.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_Unauthorized_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Unauthorized, """{"type":"AuthenticationError","message":"Authentication required"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Status.GetAsync(new(), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Unauthorized && e.ErrorType == "AuthenticationError");
	}
}
