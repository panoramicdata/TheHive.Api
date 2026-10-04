using System.Net;
using System.Text.Json;
using TheHive.Api.Data.EmailIntake;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class EmailIntakeTests
{
	private const string ImapMailboxJson = """{"_kind":"imap","provider":{"name":"imap","host":"imap.test","protocol":"imap","port":993,"ssl":true,"startTLS":false,"checkServerIdentity":true,"certificates":[{"data":"fake-cert","type":"pem"}]},"credential":{"email":"soc@example.com","basicAuth":{"password":"fake-pass"}},"inbox":"Inbox","archive":"Archive","markAsRead":true}""";

	private const string ApiMailboxJson = """{"_kind":"api","provider":{"name":"office365"},"credential":{"email":"soc@example.com","oAuth2":{"clientId":"cid","tenantId":"tid","secret":"fake-secret","authority":"https://login.test","scopes":["Mail.Read"],"redirectUri":"https://app.test/cb","authorizationCode":"fake-code"}},"inbox":"Inbox","markAsRead":false}""";

	private const string ImapConfigJson = $$"""
		{
			"id":"~4096","name":"SOC mailbox","mailbox":{{ImapMailboxJson}},"organisations":["Org"],"enabled":true,
			"createdAt":1748739600000,"alertProperties":{"type":"email-intake","source":"soc","tags":["email"]}
		}
		""";

	private const string MinimalConfigJson = $$"""
		{
			"id":"~1","name":"n","mailbox":{"_kind":"api","provider":{"name":"office365"},"credential":{"email":"e"},"inbox":"Inbox","markAsRead":false},
			"enabled":false,"createdAt":1748739600000,"alertProperties":{"type":"email-intake","source":"s"}
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

	private static EmailIntakeConfigInput FullInput() => new()
	{
		Id = "~4096",
		Name = "SOC mailbox",
		Mailbox = new EmailIntakeMailbox
		{
			Kind = EmailIntakeMailboxKinds.Imap,
			Provider = new EmailIntakeProvider
			{
				Name = "imap",
				Host = "imap.test",
				Protocol = "imap",
				Port = 993,
				Ssl = true,
				StartTls = false,
				CheckServerIdentity = true,
				Certificates = [new EmailIntakeCertificate { Data = "fake-cert", Type = "pem" }]
			},
			Credential = new EmailIntakeCredential
			{
				Email = "soc@example.com",
				BasicAuth = new EmailIntakeBasicAuth { Password = "fake-pass" }
			},
			Inbox = "Inbox",
			Archive = "Archive",
			MarkAsRead = true
		},
		Organisations = ["Org"],
		Enabled = true,
		CreatedAt = DateTimeOffset.FromUnixTimeMilliseconds(1748739600000),
		AlertProperties = new EmailIntakeAlertProperties { Type = "email-intake", Source = "soc", Tags = ["email"] }
	};

	private const string FullInputBody = $$$"""{"id":"~4096","name":"SOC mailbox","mailbox":{{{ImapMailboxJson}}},"organisations":["Org"],"enabled":true,"createdAt":1748739600000,"alertProperties":{"type":"email-intake","source":"soc","tags":["email"]}}""";

	private static EmailIntakeConfigInput MinimalInput() => new()
	{
		Name = "n",
		Mailbox = new EmailIntakeMailbox
		{
			Kind = EmailIntakeMailboxKinds.Api,
			Provider = new EmailIntakeProvider { Name = "office365" },
			Credential = new EmailIntakeCredential { Email = "e" }
		}
	};

	private const string MinimalInputBody = """{"name":"n","mailbox":{"_kind":"api","provider":{"name":"office365"},"credential":{"email":"e"}}}""";

	private static void AssertImapConfig(EmailIntakeConfig config)
	{
		config.Id.Should().Be("~4096");
		config.Name.Should().Be("SOC mailbox");
		config.Organisations.Should().Equal("Org");
		config.Enabled.Should().BeTrue();
		config.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		config.AlertProperties.Type.Should().Be("email-intake");
		config.AlertProperties.Source.Should().Be("soc");
		config.AlertProperties.Tags.Should().Equal("email");
		var mailbox = config.Mailbox;
		mailbox.Kind.Should().Be("imap");
		mailbox.Provider!.Name.Should().Be("imap");
		mailbox.Provider!.Host.Should().Be("imap.test");
		mailbox.Provider!.Protocol.Should().Be("imap");
		mailbox.Provider!.Port.Should().Be(993);
		mailbox.Provider!.Ssl.Should().BeTrue();
		mailbox.Provider!.StartTls.Should().BeFalse();
		mailbox.Provider!.CheckServerIdentity.Should().BeTrue();
		var certificate = mailbox.Provider!.Certificates.Should().ContainSingle().Subject;
		certificate.Data.Should().Be("fake-cert");
		certificate.Type.Should().Be("pem");
		mailbox.Credential!.Email.Should().Be("soc@example.com");
		mailbox.Credential!.BasicAuth!.Password.Should().Be("fake-pass");
		mailbox.Credential!.OAuth2.Should().BeNull();
		mailbox.Inbox.Should().Be("Inbox");
		mailbox.Archive.Should().Be("Archive");
		mailbox.MarkAsRead.Should().BeTrue();
		mailbox.AdditionalProperties.Should().BeNull();
	}

	[Fact]
	public async Task ListProvidersAsync_Gets_AndMapsEveryField()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			"""[{"name":"imap","_kind":"imap"},{"name":"office365","_kind":"api"},{"name":"google-workspace","_kind":"api"},{"name":"MSGraph365","_kind":"api"},{"name":"newcomer","_kind":"smtp"}]""");
		using var client = TestClient.Create(stub);

		var providers = await client.EmailIntake.ListProvidersAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/providers");
		stub.Calls[0].Body.Should().BeNull();
		providers.Select(p => p.Name).Should().Equal(
			EmailIntakeProviderName.Imap,
			EmailIntakeProviderName.Office365,
			EmailIntakeProviderName.GoogleWorkspace,
			EmailIntakeProviderName.MsGraph365,
			EmailIntakeProviderName.Unknown);
		providers.Select(p => p.Kind).Should().Equal(
			EmailIntakeConfigKind.Imap,
			EmailIntakeConfigKind.Api,
			EmailIntakeConfigKind.Api,
			EmailIntakeConfigKind.Api,
			EmailIntakeConfigKind.Unknown);
	}

	[Fact]
	public async Task CreateConfigAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, ImapConfigJson);
		using var client = TestClient.Create(stub);

		var config = await client.EmailIntake.CreateConfigAsync(FullInput(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be(FullInputBody);
		AssertImapConfig(config);
	}

	[Fact]
	public async Task CreateConfigAsync_RequiredOnly_OmitsOptionals_AndMinimalResponseMapsToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalConfigJson);
		using var client = TestClient.Create(stub);

		var config = await client.EmailIntake.CreateConfigAsync(MinimalInput(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(MinimalInputBody);
		config.Organisations.Should().BeNull();
		config.Enabled.Should().BeFalse();
		config.AlertProperties.Tags.Should().BeNull();
		config.Mailbox.Kind.Should().Be("api");
		config.Mailbox.Provider!.Name.Should().Be("office365");
		config.Mailbox.Provider!.Host.Should().BeNull();
		config.Mailbox.Credential!.BasicAuth.Should().BeNull();
		config.Mailbox.Archive.Should().BeNull();
	}

	[Fact]
	public async Task DeleteConfigAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.EmailIntake.DeleteConfigAsync("~4096", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config/~4096");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetConfigAsync_Gets_AndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, ImapConfigJson);
		using var client = TestClient.Create(stub);

		var config = await client.EmailIntake.GetConfigAsync("~4096", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config/~4096");
		stub.Calls[0].Body.Should().BeNull();
		AssertImapConfig(config);
	}

	[Fact]
	public async Task GetConfigAsync_ApiMailbox_MapsTheOAuth2Credentials()
	{
		var stub = Stub(HttpStatusCode.OK, $$$"""{"id":"~2","name":"n","mailbox":{{{ApiMailboxJson}}},"enabled":true,"createdAt":1748739600000,"alertProperties":{"type":"t","source":"s"}}""");
		using var client = TestClient.Create(stub);

		var config = await client.EmailIntake.GetConfigAsync("~2", TestContext.Current.CancellationToken);

		var oAuth2 = config.Mailbox.Credential!.OAuth2!;
		oAuth2.ClientId.Should().Be("cid");
		oAuth2.TenantId.Should().Be("tid");
		oAuth2.Secret.Should().Be("fake-secret");
		oAuth2.Authority.Should().Be("https://login.test");
		oAuth2.Scopes.Should().Equal("Mail.Read");
		oAuth2.RedirectUri.Should().Be("https://app.test/cb");
		oAuth2.AuthorizationCode.Should().Be("fake-code");
		config.Mailbox.Credential!.BasicAuth.Should().BeNull();
		config.Mailbox.MarkAsRead.Should().BeFalse();
	}

	[Fact]
	public async Task UpdateConfigAsync_PutsBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.EmailIntake.UpdateConfigAsync("~4096", FullInput(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config/~4096");
		stub.Calls[0].Body.Should().Be(FullInputBody);
	}

	[Fact]
	public async Task SetAuthorizationCodeAsync_PostsTheCodeInTheBody_AndMapsTheResult()
	{
		var stub = Stub(HttpStatusCode.OK, """{"id":"~8192","authorizationCode":"fake-code"}""");
		using var client = TestClient.Create(stub);

		var result = await client.EmailIntake.SetAuthorizationCodeAsync(
			new EmailIntakeAuthorizationCodeRequest { AuthorizationCode = "fake-code" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config/authorizationCode");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"authorizationCode":"fake-code"}""");
		result.Id.Should().Be("~8192");
		result.AuthorizationCode.Should().Be("fake-code");
	}

	[Fact]
	public async Task TestConfigAsync_PostsBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.EmailIntake.TestConfigAsync(MinimalInput(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/config/test");
		stub.Calls[0].Body.Should().Be(MinimalInputBody);
	}

	[Fact]
	public async Task GetConfigsAsync_Gets_AndMapsEveryField()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			$$$"""{"outputEmailIntake":{"enabled":true,"interval":"5 minutes","configs":[{{{ImapConfigJson}}}]}}""");
		using var client = TestClient.Create(stub);

		var result = await client.EmailIntake.GetConfigsAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/configs");
		stub.Calls[0].Body.Should().BeNull();
		result.EmailIntake.Enabled.Should().BeTrue();
		result.EmailIntake.Interval.Should().Be("5 minutes");
		AssertImapConfig(result.EmailIntake.Configs.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task UpdateConfigsAsync_PutsTheModuleSettings()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.EmailIntake.UpdateConfigsAsync(
			new EmailIntakeModuleUpdateRequest { Enabled = false, Interval = "10 minutes" },
			TestContext.Current.CancellationToken);
		await client.EmailIntake.UpdateConfigsAsync(new EmailIntakeModuleUpdateRequest { Interval = "1 hour" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/configs");
		stub.Calls[0].Body.Should().Be("""{"enabled":false,"interval":"10 minutes"}""");
		stub.Calls[1].Body.Should().Be("""{"interval":"1 hour"}""");
	}

	[Fact]
	public async Task ListFoldersAsync_PostsTheConfig_AndReturnsFolderNames()
	{
		var stub = Stub(HttpStatusCode.OK, """["Inbox","Archive","Sent"]""");
		using var client = TestClient.Create(stub);

		var folders = await client.EmailIntake.ListFoldersAsync(MinimalInput(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/folders");
		stub.Calls[0].Body.Should().Be(MinimalInputBody);
		folders.Should().Equal("Inbox", "Archive", "Sent");
	}

	[Fact]
	public async Task SyncAsync_PostsWithoutBody_AndOptionallyScopesToOneConfig()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.EmailIntake.SyncAsync(new(), TestContext.Current.CancellationToken);
		await client.EmailIntake.SyncAsync(new EmailIntakeSyncOptions { ConfigId = "~4096" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/sync");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
		stub.Calls[1].Uri.AbsolutePath.Should().Be("/api/v1/connector/email-intake/sync");
		stub.Calls[1].Uri.Query.Should().Be("?configId=~4096");
	}

	[Theory]
	[InlineData(ImapMailboxJson)]
	[InlineData(ApiMailboxJson)]
	[InlineData("""{"_kind":"graph","tokenCache":{"ttl":60}}""")]
	[InlineData("""{"_kind":"graph","provider":{"name":"newcomer"},"credential":{"email":"e"},"tokenCache":{"ttl":60}}""")]
	public void Mailbox_RoundTripsEveryKind_IncludingAnUnknownOne(string json)
		=> RoundTrip<EmailIntakeMailbox>(json).Should().Be(json);

	[Fact]
	public void Mailbox_UnknownKind_KeepsItsMembersInAdditionalProperties()
	{
		var mailbox = JsonSerializer.Deserialize<EmailIntakeMailbox>("""{"_kind":"graph","tokenCache":{"ttl":60}}""", TheHiveJson.Options)!;

		mailbox.Kind.Should().Be("graph");
		mailbox.AdditionalProperties!["tokenCache"].GetProperty("ttl").GetInt32().Should().Be(60);
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var config = new EmailIntakeConfig();
		config.Id.Should().BeEmpty();
		config.Name.Should().BeEmpty();
		config.Mailbox.Kind.Should().BeEmpty();
		config.Mailbox.Provider.Should().BeNull();
		config.Mailbox.Credential.Should().BeNull();
		config.AlertProperties.Source.Should().BeEmpty();

		new EmailIntakeCertificate().Data.Should().BeEmpty();
		new EmailIntakeBasicAuth().Password.Should().BeEmpty();
		var oAuth2 = new EmailIntakeOAuth2();
		oAuth2.ClientId.Should().BeEmpty();
		oAuth2.Secret.Should().BeEmpty();
		var module = new EmailIntakeModule();
		module.Interval.Should().BeEmpty();
		module.Configs.Should().BeEmpty();
		new EmailIntakeConfigsResult().EmailIntake.Configs.Should().BeEmpty();
		new EmailIntakeAuthorizationCodeResult().Id.Should().BeEmpty();
		new EmailIntakeAuthorizationCodeResult().AuthorizationCode.Should().BeEmpty();
	}

	[Fact]
	public async Task TestConfigAsync_BadCredentials_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Authentication failed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.EmailIntake.TestConfigAsync(MinimalInput(), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
