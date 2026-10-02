using System.Net;
using TheHive.Api.Data.Profiles;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ProfilesTests
{
	private const string FullProfileJson = """
		{
			"_id":"~84123","_type":"profile","_createdBy":"admin@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"type":"Organisation","name":"SOC-Tier1-Analyst",
			"permissions":["manageCase/create","manageCase/update","manageObservable","manageTask"],
			"editable":true,"forAdmin":false,"forOrg":true,"forExternal":true,"consumesLicense":true
		}
		""";

	private const string MinimalProfileJson = """
		{
			"_id":"~1","_type":"profile","_createdBy":"admin@example.com","_createdAt":1748739600000,"type":"Admin",
			"name":"p","editable":false,"forAdmin":true,"forOrg":false,"forExternal":false,"consumesLicense":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullProfile(Profile item)
	{
		item.Id.Should().Be("~84123");
		item.EntityType.Should().Be("profile");
		item.CreatedBy.Should().Be("admin@example.com");
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		item.Type.Should().Be(ProfileType.Organisation);
		item.Name.Should().Be("SOC-Tier1-Analyst");
		item.Permissions.Should().Equal("manageCase/create", "manageCase/update", "manageObservable", "manageTask");
		item.Editable.Should().BeTrue();
		item.ForAdmin.Should().BeFalse();
		item.ForOrg.Should().BeTrue();
		item.ForExternal.Should().BeTrue();
		item.ConsumesLicense.Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullProfileJson);
		using var client = TestClient.Create(stub);

		var result = await client.Profiles.CreateAsync(
			new ProfileCreateRequest
			{
				Type = ProfileType.Organisation,
				Name = "SOC-Tier1-Analyst",
				Permissions = ["manageCase/create", "manageCase/update", "manageObservable", "manageTask"]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/profile");
		stub.Calls[0].Body.Should().Be(
			"""{"type":"Organisation","name":"SOC-Tier1-Analyst","permissions":["manageCase/create","manageCase/update","manageObservable","manageTask"]}""");
		AssertFullProfile(result);
	}

	[Fact]
	public async Task CreateAsync_NameOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalProfileJson);
		using var client = TestClient.Create(stub);

		await client.Profiles.CreateAsync(new ProfileCreateRequest { Name = "p" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"p"}""");
	}

	[Theory]
	[InlineData(ProfileType.Admin, "Admin")]
	[InlineData(ProfileType.External, "External")]
	public async Task CreateAsync_SendsEachTypeAsItsWireName(ProfileType type, string wire)
	{
		var stub = Stub(HttpStatusCode.Created, MinimalProfileJson);
		using var client = TestClient.Create(stub);

		await client.Profiles.CreateAsync(new ProfileCreateRequest { Type = type, Name = "p" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"type":"{{wire}}","name":"p"}""");
	}

	[Fact]
	public async Task GetAsync_FullProfile_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullProfileJson);
		using var client = TestClient.Create(stub);

		var result = await client.Profiles.GetAsync("SOC Tier1", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/profile/SOC%20Tier1");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullProfile(result);
	}

	[Fact]
	public async Task GetAsync_AbsentOptionals_MapToDefaults_And_UnknownTypeIsTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalProfileJson.Replace("\"Admin\"", "\"Service\"", StringComparison.Ordinal));
		using var client = TestClient.Create(stub);

		var result = await client.Profiles.GetAsync("~1", TestContext.Current.CancellationToken);

		result.Type.Should().Be(ProfileType.Unknown);
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Permissions.Should().BeEmpty();
		result.ForAdmin.Should().BeTrue();
	}

	[Fact]
	public void Profile_Defaults_AreEmptyNotNull()
	{
		var item = new Profile();

		item.Id.Should().BeEmpty();
		item.EntityType.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Permissions.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesNameAndPermissions()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Profiles.UpdateAsync(
			"~84123",
			new ProfileUpdateRequest { Name = "New name", Permissions = ["manageTask"] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/profile/~84123");
		stub.Calls[0].Body.Should().Be("""{"name":"New name","permissions":["manageTask"]}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Profiles.UpdateAsync("~84123", new ProfileUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Profiles.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/profile/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_AssignedToUser_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Profile is assigned to a user"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Profiles.DeleteAsync("analyst", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
