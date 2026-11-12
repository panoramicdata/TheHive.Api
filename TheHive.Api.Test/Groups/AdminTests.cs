using System.Net;
using TheHive.Api.Data.Admin;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class AdminTests
{
	private static async Task<StubHandler> SetLogLevelAsync(string logger, string level)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Admin.SetLogLevelAsync(logger, level, TestContext.Current.CancellationToken);
		return stub;
	}

	[Fact]
	public async Task SetLogLevelAsync_PutsLoggerAndLevelInThePath()
	{
		var stub = await SetLogLevelAsync("org.thp.thehive", LogLevels.Debug);

		stub.ShouldHaveFirstCall(HttpMethod.Put, "/api/v1/admin/log/set/org.thp.thehive/DEBUG", null);
	}

	public static TheoryData<string, string> SpecLevels => new()
	{
		{ LogLevels.All, "ALL" },
		{ LogLevels.Trace, "TRACE" },
		{ LogLevels.Debug, "DEBUG" },
		{ LogLevels.Info, "INFO" },
		{ LogLevels.Warn, "WARN" },
		{ LogLevels.Error, "ERROR" },
		{ LogLevels.Off, "OFF" },
	};

	[Theory]
	[MemberData(nameof(SpecLevels))]
	public async Task SetLogLevelAsync_WritesEachSpecLevelToThePathVerbatim(string level, string wire)
	{
		var stub = await SetLogLevelAsync("root", level);

		stub.Calls[0].Uri.AbsolutePath.Should().Be($"/api/v1/admin/log/set/root/{wire}");
	}

	[Fact]
	public async Task SetLogLevelAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Admin.SetLogLevelAsync("root", LogLevels.Off, TestContext.Current.CancellationToken);

		await ApiAssert.ForbiddenAsync(act);
	}
}
