using System.Net;
using TheHive.Api.Data.CustomFields;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CustomFieldsTests
{
	private const string FullFieldJson = """
		{
			"_id":"~123456789","_type":"customField","_createdBy":"emma@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1735732800000,"_updatedAt":1735819200000,
			"name":"threat-type","displayName":"Threat Type","group":"Classification",
			"description":"Type of threat detected in the case.","type":"string",
			"options":["Malware","Intrusion","Data Leak"],"mandatory":true,"extraData":{"usage":12}
		}
		""";

	private const string MinimalFieldJson = """
		{
			"_id":"~1","_type":"customField","_createdBy":"emma@example.com","_createdAt":1735732800000,
			"name":"n","displayName":"n","group":"g","description":"d","type":"integer","options":[],"mandatory":false,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullField(CustomField item)
	{
		item.Id.Should().Be("~123456789");
		item.EntityType.Should().Be("customField");
		item.CreatedBy.Should().Be("emma@example.com");
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1735732800000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1735819200000));
		item.Name.Should().Be("threat-type");
		item.DisplayName.Should().Be("Threat Type");
		item.Group.Should().Be("Classification");
		item.Description.Should().Be("Type of threat detected in the case.");
		item.Type.Should().Be(CustomFieldType.String);
		item.Options.Select(o => o.GetString()).Should().Equal("Malware", "Intrusion", "Data Leak");
		item.Mandatory.Should().BeTrue();
		item.ExtraData["usage"].GetInt32().Should().Be(12);
	}

	[Fact]
	public async Task ListAsync_GetsAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, $"[{FullFieldJson},{MinimalFieldJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.CustomFields.ListAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customField");
		stub.Calls[0].Body.Should().BeNull();
		result.Should().HaveCount(2);
		AssertFullField(result[0]);
		result[1].UpdatedBy.Should().BeNull();
		result[1].UpdatedAt.Should().BeNull();
		result[1].Type.Should().Be(CustomFieldType.Integer);
		result[1].Options.Should().BeEmpty();
		result[1].Mandatory.Should().BeFalse();
	}

	[Fact]
	public async Task ListAsync_NumericOptionsAndUnknownType_AreTolerated()
	{
		var stub = Stub(HttpStatusCode.OK, """[{"_id":"~2","_type":"customField","_createdBy":"a","_createdAt":1,"name":"n","displayName":"n","group":"g","description":"d","type":"hash","options":[1.5,2],"mandatory":false,"extraData":{}}]""");
		using var client = TestClient.Create(stub);

		var result = await client.CustomFields.ListAsync(TestContext.Current.CancellationToken);

		result[0].Type.Should().Be(CustomFieldType.Unknown);
		result[0].Options.Select(o => o.GetDouble()).Should().Equal(1.5, 2);
	}

	[Theory]
	[InlineData(CustomFieldType.String, "string")]
	[InlineData(CustomFieldType.Integer, "integer")]
	[InlineData(CustomFieldType.Float, "float")]
	[InlineData(CustomFieldType.Boolean, "boolean")]
	[InlineData(CustomFieldType.Date, "date")]
	[InlineData(CustomFieldType.Url, "url")]
	public async Task CreateAsync_SendsEachTypeAsItsWireName(CustomFieldType type, string wire)
	{
		var stub = Stub(HttpStatusCode.Created, MinimalFieldJson);
		using var client = TestClient.Create(stub);

		await client.CustomFields.CreateAsync(
			new CustomFieldCreateRequest { Name = "n", Group = "g", Description = "d", Type = type },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"name":"n","group":"g","description":"d","type":"{{wire}}"}""");
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullFieldJson);
		using var client = TestClient.Create(stub);

		var result = await client.CustomFields.CreateAsync(
			new CustomFieldCreateRequest
			{
				Name = "threat-type",
				DisplayName = "Threat Type",
				Group = "Classification",
				Description = "Type of threat detected in the case.",
				Type = CustomFieldType.String,
				Mandatory = true,
				Options = ["Malware", "Intrusion", "Data Leak"]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customField");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"threat-type","displayName":"Threat Type","group":"Classification","description":"Type of threat detected in the case.","type":"string","mandatory":true,"options":["Malware","Intrusion","Data Leak"]}""");
		AssertFullField(result);
	}

	[Fact]
	public async Task CreateAsync_NumericOptions_AreSentAsNumbers()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalFieldJson);
		using var client = TestClient.Create(stub);

		await client.CustomFields.CreateAsync(
			new CustomFieldCreateRequest { Name = "n", Group = "g", Description = "d", Type = CustomFieldType.Float, Options = [1, 2.5] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"n","group":"g","description":"d","type":"float","options":[1,2.5]}""");
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CustomFields.UpdateAsync(
			"threat type",
			new CustomFieldUpdateRequest
			{
				DisplayName = "Threat",
				Group = "Classification",
				Description = "New description",
				Type = CustomFieldType.Integer,
				Mandatory = false,
				Options = [1, 2, 3]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customField/threat%20type");
		stub.Calls[0].Body.Should().Be(
			"""{"displayName":"Threat","group":"Classification","description":"New description","type":"integer","mandatory":false,"options":[1,2,3]}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CustomFields.UpdateAsync("~123456789", new CustomFieldUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_WithoutForce_SendsNoQuery()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CustomFields.DeleteAsync("~123456789", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/customField/~123456789");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_Force_SendsLowercaseBoolean()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CustomFields.DeleteAsync("~123456789", true, TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?force=true");
	}

	[Fact]
	public void CustomField_Defaults_AreEmptyNotNull()
	{
		var item = new CustomField();

		item.Id.Should().BeEmpty();
		item.EntityType.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.DisplayName.Should().BeEmpty();
		item.Group.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.Options.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task DeleteAsync_InUse_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Custom field is in use"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.CustomFields.DeleteAsync("threat-type", cancellationToken: TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
