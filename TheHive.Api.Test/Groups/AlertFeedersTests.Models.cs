using System.Net;
using System.Text.Json;
using TheHive.Api.Data.AlertFeeders;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class AlertFeedersTests
{
	[Fact]
	public void ProxyConfig_RoundTripsEveryProperty()
	{
		JsonSerializer.Serialize(FullProxy(), TheHiveJson.Options).Should().Be(ProxyJson);
		RoundTrip<ClientProxyConfig>(ProxyJson).Should().Be(ProxyJson);
	}

	[Theory]
	[InlineData("""{"type":"none"}""")]
	[InlineData("""{"type":"basic","username":"feeder","password":"fake-pass"}""")]
	[InlineData("""{"type":"bearer","key":"fake-key"}""")]
	[InlineData("""{"type":"key","key":"fake-key","prefix":"Token"}""")]
	[InlineData(OAuth2Json)]
	[InlineData("""{"type":"saml","assertion":"fake-assertion","nested":{"a":[1,2]}}""")]
	public void Auth_RoundTripsEveryKind_IncludingAnUnknownOne(string json)
		=> RoundTrip<AlertFeederAuth>(json).Should().Be(json);

	[Fact]
	public void Auth_OAuth2_MapsEveryField()
	{
		var auth = JsonSerializer.Deserialize<AlertFeederAuth>(OAuth2Json, TheHiveJson.Options)!;

		auth.Type.Should().Be("oauth2");
		auth.ClientId.Should().Be("cid");
		auth.ClientSecret.Should().Be("fake-secret");
		auth.GrantType.Should().Be(OAuthGrantType.ClientCredentials);
		auth.TokenUrl.Should().Be("https://idp.test/token");
		auth.Scope.Should().Equal("read");
		auth.ClientAuthenticationMethod.Should().Be(OAuthClientAuthenticationMethod.ClientSecretBasic);
		var parameter = auth.TokenParameters.Should().ContainSingle().Subject;
		parameter.Key.Should().Be("audience");
		parameter.Value.Should().Be("api");
		auth.AdditionalProperties.Should().BeNull();
		JsonSerializer.Serialize(FullOAuth2(), TheHiveJson.Options).Should().Be(OAuth2Json);
	}

	[Fact]
	public void Auth_UnknownKind_KeepsItsMembersInAdditionalProperties()
	{
		var auth = JsonSerializer.Deserialize<AlertFeederAuth>("""{"type":"saml","assertion":"fake-assertion"}""", TheHiveJson.Options)!;

		auth.Type.Should().Be("saml");
		auth.AdditionalProperties!["assertion"].GetString().Should().Be("fake-assertion");
	}

	[Fact]
	public void UnknownEnumValues_MapToUnknown()
	{
		var feeder = JsonSerializer.Deserialize<AlertFeeder>(
			"""{"method":"PATCH","interval":{"value":1,"unit":"Fortnights"}}""",
			TheHiveJson.Options)!;
		feeder.Method.Should().Be(AlertFeederMethod.Unknown);
		feeder.Interval.Unit.Should().Be(IntervalUnit.Unknown);

		var auth = JsonSerializer.Deserialize<AlertFeederAuth>(
			"""{"type":"oauth2","grantType":"password","clientAuthenticationMethod":"private_key_jwt"}""",
			TheHiveJson.Options)!;
		auth.GrantType.Should().Be(OAuthGrantType.Unknown);
		auth.ClientAuthenticationMethod.Should().Be(OAuthClientAuthenticationMethod.Unknown);

		var proxy = JsonSerializer.Deserialize<ClientProxyServer>("""{"state":"sometimes"}""", TheHiveJson.Options)!;
		proxy.State.Should().Be(ClientProxyState.Unknown);
	}

	[Fact]
	public async Task ListAsync_ObjectBody_AsInTheSpecExample_IsReadAsAnObject()
	{
		var stub = Stub(HttpStatusCode.OK, "[" + MinimalFeederJson.Replace("\"enabled\":false", "\"body\":{\"maxRecords\":100},\"enabled\":false", StringComparison.Ordinal) + "]");
		using var client = TestClient.Create(stub);

		var feeder = (await client.AlertFeeders.ListAsync(TestContext.Current.CancellationToken)).Should().ContainSingle().Subject;

		feeder.Body!.Value.ValueKind.Should().Be(JsonValueKind.Object);
		feeder.Body.Value.GetProperty("maxRecords").GetInt32().Should().Be(100);
	}

	[Fact]
	public async Task ListAsync_StringBody_IsStillReadAsAString()
	{
		var stub = Stub(HttpStatusCode.OK, "[" + MinimalFeederJson.Replace("\"enabled\":false", "\"body\":\"raw\",\"enabled\":false", StringComparison.Ordinal) + "]");
		using var client = TestClient.Create(stub);

		var feeder = (await client.AlertFeeders.ListAsync(TestContext.Current.CancellationToken)).Should().ContainSingle().Subject;

		feeder.Body!.Value.GetString().Should().Be("raw");
	}

	[Fact]
	public async Task CreateUpdateTest_WriteAnObjectBodyAsAnObject_AndAStringBodyAsAString()
	{
		var stub = new StubHandler();
		for (var i = 0; i < 6; i++)
		{
			stub.Enqueue(HttpStatusCode.OK, i == 5 ? "x" : MinimalFeederJson);
		}

		using var client = TestClient.Create(stub);
		var objectBody = JsonSerializer.SerializeToElement(new { maxRecords = 100 });
		var stringBody = JsonSerializer.SerializeToElement("raw");
		var interval = new Interval { Value = 1, Unit = IntervalUnit.Hours };

		foreach (var body in new[] { objectBody, stringBody })
		{
			await client.AlertFeeders.CreateAsync(
				new AlertFeederCreateRequest { Name = "f", Description = "d", Method = AlertFeederMethod.Post, Url = "u", Interval = interval, FunctionName = "fn", Body = body },
				TestContext.Current.CancellationToken);
			await client.AlertFeeders.UpdateAsync(
				"f",
				new AlertFeederUpdateRequest { Description = "d", Method = AlertFeederMethod.Post, Url = "u", Interval = interval, Body = body },
				TestContext.Current.CancellationToken);
		}

		await client.AlertFeeders.TestAsync(
			new AlertFeederTestRequest { Name = "f", Description = "d", Method = AlertFeederMethod.Post, Url = "u", Interval = interval, Body = objectBody },
			TestContext.Current.CancellationToken);
		await client.AlertFeeders.TestAsync(
			new AlertFeederTestRequest { Name = "f", Description = "d", Method = AlertFeederMethod.Post, Url = "u", Interval = interval, Body = stringBody },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Contain("\"body\":{\"maxRecords\":100}");
		stub.Calls[1].Body.Should().Contain("\"body\":{\"maxRecords\":100}");
		stub.Calls[2].Body.Should().Contain("\"body\":\"raw\"");
		stub.Calls[3].Body.Should().Contain("\"body\":\"raw\"");
		stub.Calls[4].Body.Should().Contain("\"body\":{\"maxRecords\":100}");
		stub.Calls[5].Body.Should().Contain("\"body\":\"raw\"");
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var feeder = new AlertFeeder();
		feeder.Name.Should().BeEmpty();
		feeder.Description.Should().BeEmpty();
		feeder.Url.Should().BeEmpty();
		feeder.Interval.Value.Should().Be(0);
		feeder.Function.Id.Should().BeEmpty();
		feeder.RequestTimeout.Unit.Should().Be(IntervalUnit.Unknown);

		new AlertFeederHeader().Key.Should().BeEmpty();
		new AlertFeederHeader().Value.Should().BeEmpty();
		new AlertFeederAuth().Type.Should().BeEmpty();
		new OAuthTokenParameter().Key.Should().BeEmpty();
		new OAuthTokenParameter().Value.Should().BeEmpty();
	}

	[Fact]
	public async Task ListAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.AlertFeeders.ListAsync(TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
