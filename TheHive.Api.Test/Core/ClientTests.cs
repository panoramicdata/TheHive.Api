using System.Net;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public class ClientTests
{
	[Theory]
	[InlineData("https://hive.test/thehive", "https://hive.test/thehive/api/v1/case")]
	[InlineData("https://hive.test/thehive/", "https://hive.test/thehive/api/v1/case")]
	[InlineData("https://hive.test", "https://hive.test/api/v1/case")]
	[InlineData("https://hive.test/", "https://hive.test/api/v1/case")]
	public async Task RelativePath_ResolvesUnderBaseUrl(string baseUrl, string expected)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub, o => o.BaseUrl = baseUrl);

		using var response = await client.HttpClient.GetAsync("api/v1/case", TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle().Which.Uri.ToString().Should().Be(expected);
	}

	[Fact]
	public async Task ExceptionFactory_ErrorResponse_YieldsTheHiveApiException()
	{
		using var client = TestClient.Create(new StubHandler());
		using var response = new HttpResponseMessage(HttpStatusCode.NotFound)
		{
			Content = new StringContent("""{"type":"NotFound","message":"nope"}""")
		};

		var exception = await client.Settings.ExceptionFactory(response);

		exception.Should().BeOfType<TheHiveApiException>().Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
	}

	[Fact]
	public async Task ExceptionFactory_SuccessResponse_YieldsNull()
	{
		using var client = TestClient.Create(new StubHandler());
		using var response = new HttpResponseMessage(HttpStatusCode.OK);

		(await client.Settings.ExceptionFactory(response)).Should().BeNull();
	}

	[Fact]
	public void PublicConstructor_BuildsWithoutThrowing()
	{
		var act = () => new TheHiveClient(new TheHiveClientOptions { BaseUrl = "https://hive.test", ApiKey = "k" }).Dispose();
		act.Should().NotThrow();
	}

	[Fact]
	public void InvalidOptions_Throw()
	{
		var act = () => new TheHiveClient(new TheHiveClientOptions());
		act.Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Dispose_Twice_DoesNotThrow()
	{
		var client = TestClient.Create(new StubHandler());
		client.Dispose();
		client.Invoking(c => c.Dispose()).Should().NotThrow();
	}

	[Fact]
	public async Task Request_CarriesBearerToken()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		using var response = await client.HttpClient.GetAsync("api/v1/case", TestContext.Current.CancellationToken);

		stub.Calls.Single().Headers.Authorization!.ToString().Should().Be("Bearer fake-key");
	}
}
