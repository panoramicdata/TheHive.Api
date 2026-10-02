namespace TheHive.Api.Test.Integration;

public class ReadOnlyTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task GetCurrentUser_ReturnsLogin()
	{
		var user = await Client.Users.GetCurrentAsync(CancellationToken);

		user.Login.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task ListPermissions_ReturnsAtLeastOne()
	{
		var permissions = await Client.Permissions.ListAsync(CancellationToken);

		permissions.Should().NotBeEmpty();
		permissions.Should().OnlyContain(p => !string.IsNullOrEmpty(p.Name));
	}
}
