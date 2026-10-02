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

	[Fact]
	public async Task GetPublicStatus_ReturnsVersion()
	{
		var status = await Client.Status.GetPublicAsync(CancellationToken);

		status.Version.Should().NotBeNullOrWhiteSpace();
		status.Imports.MitreCatalog.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task GetStatus_ReturnsVersionAndLicense()
	{
		var status = await Client.Status.GetAsync(cancellationToken: CancellationToken);

		status.Version.Should().NotBeNullOrWhiteSpace();
		status.License.Plan.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task GetCurrentLicense_ReturnsAValidLicenseOrFallback()
	{
		var current = await Client.License.GetCurrentAsync(CancellationToken);

		(current.License ?? current.Fallback).Should().NotBeNull();
	}
}
