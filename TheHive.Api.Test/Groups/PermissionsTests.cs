using System.Net;
using TheHive.Api.Data.Permissions;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class PermissionsTests
{
	[Fact]
	public async Task ListAsync_GetsAndMapsEveryField()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, """
			[
				{"name":"manageCase/create","label":"Create cases","consumesLicense":true,"scope":["organisation"]},
				{"name":"manageSomethingNew","label":"New","consumesLicense":false}
			]
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Permissions.ListAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/permission");
		stub.Calls[0].Body.Should().BeNull();
		result.Should().HaveCount(2);
		result[0].Name.Should().Be("manageCase/create");
		result[0].Label.Should().Be("Create cases");
		result[0].ConsumesLicense.Should().BeTrue();
		result[0].Scope.Should().Equal("organisation");
		result[1].Name.Should().Be("manageSomethingNew");
		result[1].ConsumesLicense.Should().BeFalse();
		result[1].Scope.Should().BeEmpty();
	}

	[Fact]
	public void PermissionDescription_Defaults_AreEmptyNotNull()
	{
		var item = new PermissionDescription();

		item.Name.Should().BeEmpty();
		item.Label.Should().BeEmpty();
		item.Scope.Should().BeEmpty();
	}

	[Fact]
	public async Task ListAsync_Unauthorized_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Unauthorized, """{"type":"AuthenticationError","message":"Authentication failure"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Permissions.ListAsync(TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Unauthorized && e.ErrorType == "AuthenticationError");
	}
}
