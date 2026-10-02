using System.Net;
using TheHive.Api.Data.Authentication;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class AuthenticationTests
{
	private const string UserJson = """
		{
			"_id":"~192024","_createdBy":"admin@example.com","_createdAt":1748739600000,"login":"lucas@example.com","name":"Sami Analyst",
			"hasKey":true,"hasPassword":true,"hasMFA":true,"locked":false,"profile":"analyst","organisation":"TheOrganization",
			"type":"Normal","extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task GetPasswordPolicyAsync_MapsEveryRule()
	{
		var stub = Stub(
			HttpStatusCode.OK,
			"""{"enabled":true,"minLength":12,"minLowerCase":1,"minUpperCase":2,"minDigit":3,"minSpecial":4,"cannotContainUsername":true}""");
		using var client = TestClient.Create(stub);

		var result = await client.Authentication.GetPasswordPolicyAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/auth/local/passwordPolicy");
		stub.Calls[0].Body.Should().BeNull();
		result.Enabled.Should().BeTrue();
		result.MinLength.Should().Be(12);
		result.MinLowerCase.Should().Be(1);
		result.MinUpperCase.Should().Be(2);
		result.MinDigit.Should().Be(3);
		result.MinSpecial.Should().Be(4);
		result.CannotContainUsername.Should().BeTrue();
	}

	[Fact]
	public async Task GetPasswordPolicyAsync_NoRulesConfigured_MapsNulls()
	{
		var stub = Stub(HttpStatusCode.OK, """{"enabled":false}""");
		using var client = TestClient.Create(stub);

		var result = await client.Authentication.GetPasswordPolicyAsync(TestContext.Current.CancellationToken);

		result.Enabled.Should().BeFalse();
		result.MinLength.Should().BeNull();
		result.MinLowerCase.Should().BeNull();
		result.MinUpperCase.Should().BeNull();
		result.MinDigit.Should().BeNull();
		result.MinSpecial.Should().BeNull();
		result.CannotContainUsername.Should().BeNull();
	}

	[Fact]
	public async Task GetTotpSecretAsync_MapsSecretAndUri()
	{
		var stub = Stub(HttpStatusCode.OK, """{"secret":"fake-totp-secret","uri":"otpauth://totp/TheHive:lucas@example.com?secret=fake-totp-secret&issuer=TheHive"}""");
		using var client = TestClient.Create(stub);

		var result = await client.Authentication.GetTotpSecretAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/auth/totp/get");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		result.Secret.Should().Be("fake-totp-secret");
		result.Uri.Should().Be("otpauth://totp/TheHive:lucas@example.com?secret=fake-totp-secret&issuer=TheHive");
	}

	[Fact]
	public void TotpSecret_Defaults_AreEmptyNotNull()
	{
		var secret = new TotpSecret();

		secret.Secret.Should().BeEmpty();
		secret.Uri.Should().BeEmpty();
	}

	[Fact]
	public async Task SetTotpAsync_PostsCodeAndSecretInTheBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Authentication.SetTotpAsync(new TotpActivateRequest { Code = 123456, Secret = "fake-totp-secret" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/auth/totp/set");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"code":123456,"secret":"fake-totp-secret"}""");
	}

	[Fact]
	public async Task UnsetTotpAsync_PostsWithoutBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Authentication.UnsetTotpAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/auth/totp/unset");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task UnsetTotpForUserAsync_PostsTheLoginAsOnePathSegment()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Authentication.UnsetTotpForUserAsync("lucas@example.com", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/auth/totp/unset/lucas%40example.com");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task LoginAsync_PostsCredentialsInTheBody_KeepsApiKeyAuth_AndMapsTheUser()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, UserJson, r => r.Headers.Add("Set-Cookie", "THEHIVE-SESSION=fake-session; Path=/; HttpOnly"));
		using var client = TestClient.Create(stub);

		var result = await client.Authentication.LoginAsync(
			new LoginRequest { User = "lucas@example.com", Password = "fake-password", Organisation = "TheOrganization", Code = "000000" },
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/login");
		call.Uri.Query.Should().BeEmpty();
		call.Body.Should().Be("""{"user":"lucas@example.com","password":"fake-password","organisation":"TheOrganization","code":"000000"}""");
		call.Headers.Authorization!.ToString().Should().Be("Bearer fake-key");
		result.Login.Should().Be("lucas@example.com");
		result.Name.Should().Be("Sami Analyst");
		result.Profile.Should().Be("analyst");
		result.Organisation.Should().Be("TheOrganization");
		result.HasMfa.Should().BeTrue();
	}

	[Fact]
	public async Task LoginAsync_RequiredFieldsOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.OK, UserJson);
		using var client = TestClient.Create(stub);

		await client.Authentication.LoginAsync(new LoginRequest { User = "u@example.com", Password = "fake-password" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"user":"u@example.com","password":"fake-password"}""");
	}

	[Fact]
	public async Task LogoutByGetAsync_SendsGet()
	{
		var stub = Stub(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.Authentication.LogoutByGetAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/logout");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task LogoutAsync_SendsPostWithoutBody()
	{
		var stub = Stub(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.Authentication.LogoutAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/logout");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task LoginAsync_WrongCredentials_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Unauthorized, """{"type":"AuthenticationError","message":"Authentication failure"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Authentication.LoginAsync(new LoginRequest { User = "u@example.com", Password = "fake-wrong" }, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Unauthorized && e.ErrorType == "AuthenticationError");
	}
}
