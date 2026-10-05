using System.Net;
using System.Text.Json;
using TheHive.Api.Data.AlertFeeders;
using TheHive.Api.Data.Common;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class AlertFeedersTests
{
	private const string ProxyJson = """{"timeout":{"connection":"10 seconds","idle":"30 seconds","request":"60 seconds"},"followRedirects":true,"useProxyProperties":false,"userAgent":"feeder","compressionEnabled":true,"ssl":{"default":false,"protocol":"TLSv1.2","checkRevocation":true,"revocationLists":["crl1"],"enabledCipherSuites":["suite"],"enabledProtocols":["TLSv1.3"],"hostnameVerifierClass":"NoopHostnameVerifier","secureRandom":"rand","trustManager":{"algorithm":"PKIX","stores":[{"data":"fake-pem","filePath":"/etc/trust.pem","isFileOnClasspath":false,"password":"fake-store-pass","type":"PEM"}]},"keyManager":{"algorithm":"SunX509"},"sslParametersConfig":{"clientAuth":"Want","protocols":["TLSv1.2"]},"debug":{"all":false,"keymanager":true,"ssl":false,"sslctx":true,"trustmanager":false},"loose":{"acceptAnyCertificate":false,"allowLegacyHelloMessages":true,"allowUnsafeRenegotiation":false,"disableHostnameVerification":true,"disableSNI":false}},"maxConnectionsPerHost":-1,"maxConnectionsTotal":-1,"maxConnectionLifetime":"Duration.Inf","idleConnectionInPoolTimeout":"1 minute","connectionPoolCleanerPeriod":"1 minute","maxNumberOfRedirects":5,"maxRequestRetry":5,"disableUrlEncoding":false,"keepAlive":true,"useLaxCookieEncoder":false,"useCookieStore":false,"proxy":{"host":"proxy.test","port":3128,"state":"enabled","protocol":"http","principal":"proxy-user","password":"fake-proxy-pass","ntlmDomain":"DOM","encoding":"UTF-8","nonProxyHosts":["localhost"]}}""";

	private const string OAuth2Json = """{"type":"oauth2","clientId":"cid","clientSecret":"fake-secret","grantType":"client_credentials","tokenUrl":"https://idp.test/token","scope":["read"],"clientAuthenticationMethod":"client_secret_basic","tokenParameters":[{"key":"audience","value":"api"}]}""";

	private const string FeederJson = $$"""
		{
			"name":"threat-feed","description":"Pulls indicators","method":"POST","url":"https://feed.test/api","interval":{"value":5,"unit":"Minutes"},
			"function":{"_id":"~84123","_type":"Function","_createdBy":"emma@example.com","_createdAt":1748739600000,"name":"feed-fn","mode":"Enabled","definition":"x","config":{},"types":["feeder:alert"]},
			"headers":[{"key":"X-Feed","value":"fake-header"}],
			"auth":{"type":"basic","username":"feeder","password":"fake-pass"},
			"body":{"maxRecords":100},"enabled":true,"requestTimeout":{"value":10,"unit":"Seconds"},"responseMaxSize":10485760,
			"proxyConfig":{{ProxyJson}}
		}
		""";

	private const string MinimalFeederJson = """
		{
			"name":"f","description":"d","method":"GET","url":"https://feed.test","interval":{"value":1,"unit":"Hours"},
			"function":{"_id":"~1","_type":"Function","_createdBy":"emma@example.com","_createdAt":1748739600000,"name":"fn","mode":"Enabled","definition":"x","config":{}},
			"enabled":false,"requestTimeout":{"value":10,"unit":"Seconds"},"responseMaxSize":1
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static string RoundTrip<T>(string json)
		=> JsonSerializer.Serialize(JsonSerializer.Deserialize<T>(json, TheHiveJson.Options), TheHiveJson.Options);

	private static ClientProxyConfig FullProxy() => new()
	{
		Timeout = new ClientProxyTimeout { Connection = "10 seconds", Idle = "30 seconds", Request = "60 seconds" },
		FollowRedirects = true,
		UseProxyProperties = false,
		UserAgent = "feeder",
		CompressionEnabled = true,
		Ssl = new ClientSslConfig
		{
			Default = false,
			Protocol = "TLSv1.2",
			CheckRevocation = true,
			RevocationLists = ["crl1"],
			EnabledCipherSuites = ["suite"],
			EnabledProtocols = ["TLSv1.3"],
			HostnameVerifierClass = "NoopHostnameVerifier",
			SecureRandom = "rand",
			TrustManager = new ClientKeyManager
			{
				Algorithm = "PKIX",
				Stores =
				[
					new ClientKeyStore
					{
						Data = "fake-pem",
						FilePath = "/etc/trust.pem",
						IsFileOnClasspath = false,
						Password = "fake-store-pass",
						Type = "PEM"
					}
				]
			},
			KeyManager = new ClientKeyManager { Algorithm = "SunX509" },
			SslParametersConfig = new ClientSslParameters { ClientAuth = "Want", Protocols = ["TLSv1.2"] },
			Debug = new ClientSslDebugConfig { All = false, KeyManager = true, Ssl = false, SslContext = true, TrustManager = false },
			Loose = new ClientSslLooseConfig
			{
				AcceptAnyCertificate = false,
				AllowLegacyHelloMessages = true,
				AllowUnsafeRenegotiation = false,
				DisableHostnameVerification = true,
				DisableSni = false
			}
		},
		MaxConnectionsPerHost = -1,
		MaxConnectionsTotal = -1,
		MaxConnectionLifetime = "Duration.Inf",
		IdleConnectionInPoolTimeout = "1 minute",
		ConnectionPoolCleanerPeriod = "1 minute",
		MaxNumberOfRedirects = 5,
		MaxRequestRetry = 5,
		DisableUrlEncoding = false,
		KeepAlive = true,
		UseLaxCookieEncoder = false,
		UseCookieStore = false,
		Proxy = new ClientProxyServer
		{
			Host = "proxy.test",
			Port = 3128,
			State = ClientProxyState.Enabled,
			Protocol = "http",
			Principal = "proxy-user",
			Password = "fake-proxy-pass",
			NtlmDomain = "DOM",
			Encoding = "UTF-8",
			NonProxyHosts = ["localhost"]
		}
	};

	private static AlertFeederAuth FullOAuth2() => new()
	{
		Type = AlertFeederAuthTypes.OAuth2,
		ClientId = "cid",
		ClientSecret = "fake-secret",
		GrantType = OAuthGrantType.ClientCredentials,
		TokenUrl = "https://idp.test/token",
		Scope = ["read"],
		ClientAuthenticationMethod = OAuthClientAuthenticationMethod.ClientSecretBasic,
		TokenParameters = [new OAuthTokenParameter { Key = "audience", Value = "api" }]
	};

	[Fact]
	public async Task ListAsync_Gets_AndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{FeederJson}]");
		using var client = TestClient.Create(stub);

		var feeders = await client.AlertFeeders.ListAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder");
		stub.Calls[0].Body.Should().BeNull();
		var feeder = feeders.Should().ContainSingle().Subject;
		feeder.Name.Should().Be("threat-feed");
		feeder.Description.Should().Be("Pulls indicators");
		feeder.Method.Should().Be(AlertFeederMethod.Post);
		feeder.Url.Should().Be("https://feed.test/api");
		feeder.Interval.Value.Should().Be(5);
		feeder.Interval.Unit.Should().Be(IntervalUnit.Minutes);
		feeder.Function.Id.Should().Be("~84123");
		feeder.Function.Types.Should().Equal("feeder:alert");
		var header = feeder.Headers.Should().ContainSingle().Subject;
		header.Key.Should().Be("X-Feed");
		header.Value.Should().Be("fake-header");
		feeder.Auth!.Type.Should().Be(AlertFeederAuthTypes.Basic);
		feeder.Auth.Username.Should().Be("feeder");
		feeder.Auth.Password.Should().Be("fake-pass");
		feeder.Body!.Value.GetProperty("maxRecords").GetInt32().Should().Be(100);
		feeder.Enabled.Should().BeTrue();
		feeder.RequestTimeout.Value.Should().Be(10);
		feeder.RequestTimeout.Unit.Should().Be(IntervalUnit.Seconds);
		feeder.ResponseMaxSize.Should().Be(10485760);
		feeder.ProxyConfig!.Proxy!.Host.Should().Be("proxy.test");
		feeder.ProxyConfig.Proxy.State.Should().Be(ClientProxyState.Enabled);
		feeder.ProxyConfig.Ssl!.Loose!.DisableSni.Should().BeFalse();
		feeder.ProxyConfig.Ssl.TrustManager!.Stores![0].Password.Should().Be("fake-store-pass");
	}

	[Fact]
	public async Task ListAsync_MinimalFeeder_MapsAbsentOptionalsToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{MinimalFeederJson}]");
		using var client = TestClient.Create(stub);

		var feeder = (await client.AlertFeeders.ListAsync(TestContext.Current.CancellationToken)).Should().ContainSingle().Subject;

		feeder.Method.Should().Be(AlertFeederMethod.Get);
		feeder.Headers.Should().BeNull();
		feeder.Auth.Should().BeNull();
		feeder.Body.Should().BeNull();
		feeder.ProxyConfig.Should().BeNull();
		feeder.Enabled.Should().BeFalse();
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsTheFeeder()
	{
		var stub = Stub(HttpStatusCode.Created, FeederJson);
		using var client = TestClient.Create(stub);

		var feeder = await client.AlertFeeders.CreateAsync(
			new AlertFeederCreateRequest
			{
				Name = "threat-feed",
				Description = "Pulls indicators",
				Method = AlertFeederMethod.Post,
				Url = "https://feed.test/api",
				Interval = new Interval { Value = 5, Unit = IntervalUnit.Minutes },
				FunctionName = "feed-fn",
				Body = JsonSerializer.SerializeToElement("{}"),
				Headers = [new AlertFeederHeader { Key = "X-Feed", Value = "fake-header" }],
				Enabled = true,
				Auth = new AlertFeederAuth { Type = AlertFeederAuthTypes.Bearer, Key = "fake-key" },
				ProxyConfig = new ClientProxyConfig { FollowRedirects = false },
				RequestTimeout = new Interval { Value = 10, Unit = IntervalUnit.Seconds },
				ResponseMaxSize = 1048576
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be(
			"""{"name":"threat-feed","description":"Pulls indicators","method":"POST","url":"https://feed.test/api","interval":{"value":5,"unit":"Minutes"},"functionName":"feed-fn","body":"{}","headers":[{"key":"X-Feed","value":"fake-header"}],"enabled":true,"auth":{"type":"bearer","key":"fake-key"},"proxyConfig":{"followRedirects":false},"requestTimeout":{"value":10,"unit":"Seconds"},"responseMaxSize":1048576}""");
		feeder.Name.Should().Be("threat-feed");
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalFeederJson);
		using var client = TestClient.Create(stub);

		await client.AlertFeeders.CreateAsync(
			new AlertFeederCreateRequest
			{
				Name = "f",
				Description = "d",
				Method = AlertFeederMethod.Get,
				Url = "https://feed.test",
				Interval = new Interval { Value = 1, Unit = IntervalUnit.Hours },
				FunctionName = "fn"
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			"""{"name":"f","description":"d","method":"GET","url":"https://feed.test","interval":{"value":1,"unit":"Hours"},"functionName":"fn"}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.AlertFeeders.DeleteAsync("threat feed", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder/threat%20feed");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task UpdateAsync_PutsBodyAndMapsTheFeeder()
	{
		var stub = Stub(HttpStatusCode.Created, FeederJson);
		using var client = TestClient.Create(stub);

		var feeder = await client.AlertFeeders.UpdateAsync(
			"threat-feed",
			new AlertFeederUpdateRequest
			{
				Description = "New description",
				Method = AlertFeederMethod.Get,
				Url = "https://feed.test/v2",
				Interval = new Interval { Value = 1, Unit = IntervalUnit.Days },
				Body = JsonSerializer.SerializeToElement("{}"),
				Headers = [new AlertFeederHeader { Key = "X-Feed", Value = "fake-header" }],
				Enabled = false,
				Auth = FullOAuth2(),
				ProxyConfig = new ClientProxyConfig { UseCookieStore = true },
				RequestTimeout = new Interval { Value = 500, Unit = IntervalUnit.Milliseconds },
				ResponseMaxSize = 2048
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder/threat-feed");
		stub.Calls[0].Body.Should().Be(
			$$"""{"description":"New description","method":"GET","url":"https://feed.test/v2","interval":{"value":1,"unit":"Days"},"body":"{}","headers":[{"key":"X-Feed","value":"fake-header"}],"enabled":false,"auth":{{OAuth2Json}},"proxyConfig":{"useCookieStore":true},"requestTimeout":{"value":500,"unit":"Milliseconds"},"responseMaxSize":2048}""");
		feeder.Name.Should().Be("threat-feed");
	}

	[Fact]
	public async Task UpdateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalFeederJson);
		using var client = TestClient.Create(stub);

		await client.AlertFeeders.UpdateAsync(
			"f",
			new AlertFeederUpdateRequest
			{
				Description = "d",
				Method = AlertFeederMethod.Post,
				Url = "https://feed.test",
				Interval = new Interval { Value = 2, Unit = IntervalUnit.Hours }
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"description":"d","method":"POST","url":"https://feed.test","interval":{"value":2,"unit":"Hours"}}""");
	}

	[Fact]
	public async Task RunAsync_PostsWithoutBody_AndSendsDryRunLowercase()
	{
		var stub = Stub(HttpStatusCode.OK, """{"result":null,"durationMillis":12,"stdout":"out","stderr":"err"}""");
		using var client = TestClient.Create(stub);

		var result = await client.AlertFeeders.RunAsync("threat-feed", new DryRunOptions { DryRun = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder/run/threat-feed");
		stub.Calls[0].Uri.Query.Should().Be("?dryRun=true");
		stub.Calls[0].Body.Should().BeNull();
		result.DurationMillis.Should().Be(12);
		result.Stdout.Should().Be("out");
		result.Stderr.Should().Be("err");
	}

	[Fact]
	public async Task RunAsync_WithoutDryRun_OmitsTheQuery()
	{
		var stub = Stub(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub);

		await client.AlertFeeders.RunAsync("f", new(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().BeEmpty();
	}

	[Fact]
	public async Task TestAsync_PostsBody_AndReturnsTheRawResponseText()
	{
		var stub = Stub(HttpStatusCode.OK, """{"indicators":[1,2]}""");
		using var client = TestClient.Create(stub);

		var raw = await client.AlertFeeders.TestAsync(
			new AlertFeederTestRequest
			{
				Name = "threat-feed",
				Description = "d",
				Method = AlertFeederMethod.Get,
				Url = "https://feed.test",
				Interval = new Interval { Value = 1, Unit = IntervalUnit.Minutes },
				Body = JsonSerializer.SerializeToElement("b"),
				Headers = [new AlertFeederHeader { Key = "X-Feed", Value = "fake-header" }],
				Enabled = true,
				Auth = new AlertFeederAuth { Type = AlertFeederAuthTypes.None },
				ProxyConfig = new ClientProxyConfig { KeepAlive = true },
				RequestTimeout = new Interval { Value = 5, Unit = IntervalUnit.Seconds },
				ResponseMaxSize = 100
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/alert-feeder/test");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"threat-feed","description":"d","method":"GET","url":"https://feed.test","interval":{"value":1,"unit":"Minutes"},"body":"b","headers":[{"key":"X-Feed","value":"fake-header"}],"enabled":true,"auth":{"type":"none"},"proxyConfig":{"keepAlive":true},"requestTimeout":{"value":5,"unit":"Seconds"},"responseMaxSize":100}""");
		raw.Should().Be("""{"indicators":[1,2]}""");
	}

	[Fact]
	public async Task TestAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.OK, "not json");
		using var client = TestClient.Create(stub);

		var raw = await client.AlertFeeders.TestAsync(
			new AlertFeederTestRequest
			{
				Name = "f",
				Description = "d",
				Method = AlertFeederMethod.Get,
				Url = "https://feed.test",
				Interval = new Interval { Value = 1, Unit = IntervalUnit.Minutes }
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"f","description":"d","method":"GET","url":"https://feed.test","interval":{"value":1,"unit":"Minutes"}}""");
		raw.Should().Be("not json");
	}
}
