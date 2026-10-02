using System.Net;
using TheHive.Api.Data.Admin;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class AdminTests
{
	[Fact]
	public async Task SetLogLevelAsync_PutsLoggerAndLevelInThePath()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Admin.SetLogLevelAsync("org.thp.thehive", LogLevels.Debug, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/admin/log/set/org.thp.thehive/DEBUG");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Theory]
	[InlineData(LogLevels.All, "ALL")]
	[InlineData(LogLevels.Trace, "TRACE")]
	[InlineData(LogLevels.Debug, "DEBUG")]
	[InlineData(LogLevels.Info, "INFO")]
	[InlineData(LogLevels.Warn, "WARN")]
	[InlineData(LogLevels.Error, "ERROR")]
	[InlineData(LogLevels.Off, "OFF")]
	public async Task SetLogLevelAsync_WritesEachSpecLevelToThePathVerbatim(string level, string wire)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Admin.SetLogLevelAsync("root", level, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be($"/api/v1/admin/log/set/root/{wire}");
	}

	[Fact]
	public async Task SetLogLevelAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Admin.SetLogLevelAsync("root", LogLevels.Off, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
