using System.Net;
using Refit;
using TheHive.Api.Data.Taxonomies;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class TaxonomiesTests
{
	private const string TaxonomyJson = """
		{
			"_id":"~84123","_type":"Taxonomy","_createdBy":"emma@example.com","_updatedBy":"sami@example.com",
			"_createdAt":1744660800000,"_updatedAt":1744747200000,"namespace":"tlp",
			"description":"Traffic Light Protocol for information sharing","version":6,
			"tags":[{"_id":"~83456","_type":"Tag","_createdBy":"emma@example.com","_createdAt":1744660800000,
				"namespace":"tlp","predicate":"amber","value":"strict","colour":"#ffa800","hidden":false,"extraData":{}}],
			"extraData":{"enabled":true}
		}
		""";

	private const string MinimalTaxonomyJson = """
		{
			"_id":"~1","_type":"Taxonomy","_createdBy":"emma@example.com","_createdAt":1744660800000,
			"namespace":"n","description":"d","version":1,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, TaxonomyJson);
		using var client = TestClient.Create(stub);

		var item = await client.Taxonomies.CreateAsync(
			new TaxonomyCreateRequest
			{
				Namespace = "tlp",
				Description = "Traffic Light Protocol for information sharing",
				Version = 6,
				Exclusive = true,
				Predicates =
				[
					new TaxonomyPredicate
					{
						Value = "white",
						Expanded = "TLP:WHITE",
						Exclusive = false,
						Description = "Disclosure is not limited.",
						Colour = "#ffffff"
					},
					new TaxonomyPredicate { Value = "amber" }
				],
				Values =
				[
					new TaxonomyValue
					{
						Predicate = "amber",
						Entry =
						[
							new TaxonomyEntry
							{
								Value = "strict",
								Expanded = "Recipients may share information only within their organization.",
								Colour = "#ffa800",
								Description = "Restricted.",
								NumericalValue = 50
							},
							new TaxonomyEntry { Value = "plain" }
						]
					},
					new TaxonomyValue { Predicate = "red" }
				]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/taxonomy");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be(
			"""{"namespace":"tlp","description":"Traffic Light Protocol for information sharing","version":6,"exclusive":true,"predicates":[{"value":"white","expanded":"TLP:WHITE","exclusive":false,"description":"Disclosure is not limited.","colour":"#ffffff"},{"value":"amber"}],"values":[{"predicate":"amber","entry":[{"value":"strict","expanded":"Recipients may share information only within their organization.","colour":"#ffa800","description":"Restricted.","numerical_value":50},{"value":"plain"}]},{"predicate":"red"}]}""");
		item.Id.Should().Be("~84123");
		item.Type.Should().Be("Taxonomy");
		item.CreatedBy.Should().Be("emma@example.com");
		item.UpdatedBy.Should().Be("sami@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1744660800000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1744747200000));
		item.Namespace.Should().Be("tlp");
		item.Description.Should().Be("Traffic Light Protocol for information sharing");
		item.Version.Should().Be(6);
		var tag = item.Tags.Should().ContainSingle().Subject;
		tag.Id.Should().Be("~83456");
		tag.Predicate.Should().Be("amber");
		tag.Value.Should().Be("strict");
		tag.Colour.Should().Be("#ffa800");
		item.ExtraData["enabled"].GetBoolean().Should().BeTrue();
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalTaxonomyJson);
		using var client = TestClient.Create(stub);

		await client.Taxonomies.CreateAsync(
			new TaxonomyCreateRequest { Namespace = "n", Description = "d", Version = 1 },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"namespace":"n","description":"d","version":1}""");
	}

	[Fact]
	public async Task GetAsync_Gets_AndAbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalTaxonomyJson);
		using var client = TestClient.Create(stub);

		var item = await client.Taxonomies.GetAsync("my tax", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/taxonomy/my%20tax");
		stub.Calls[0].Body.Should().BeNull();
		item.UpdatedBy.Should().BeNull();
		item.UpdatedAt.Should().BeNull();
		item.Tags.Should().BeEmpty();
	}

	[Fact]
	public void Defaults_AreEmptyNotNull()
	{
		var item = new Taxonomy();
		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Namespace.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.Tags.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();

		var imported = new TaxonomyImported();
		imported.Namespace.Should().BeEmpty();
		imported.Status.Should().BeEmpty();

		var error = new TaxonomyImportError();
		error.File.Should().BeEmpty();
		error.Error.Should().BeEmpty();
		error.Status.Should().BeEmpty();
		error.Details.Should().BeNull();

		var result = new TaxonomyImportResult();
		result.Imported.Should().BeEmpty();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Taxonomies.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/taxonomy/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ActivateAsync_SendsPutWithoutBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Taxonomies.ActivateAsync("tlp", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/taxonomy/tlp/activate");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DeactivateAsync_SendsPutWithoutBody()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Taxonomies.DeactivateAsync("tlp", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/taxonomy/tlp/deactivate");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ImportZipAsync_UploadsTheArchiveAsAFilePart_And_MapsOkResult()
	{
		var stub = Stub(HttpStatusCode.Created, """{"imported":[{"namespace":"tlp","numberOfTags":28,"status":"Success"}]}""");
		using var client = TestClient.Create(stub);
		byte[] zip = [0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF];

		var result = await client.Taxonomies.ImportZipAsync(
			new ByteArrayPart(zip, "taxonomies.zip", "application/zip"),
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/taxonomy/import-zip");
		call.ContentType.Should().Be("multipart/form-data");
		var part = call.Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("file");
		part.FileName.Should().Be("taxonomies.zip");
		part.ContentType.Should().Be("application/zip");
		part.Bytes.Should().Equal(zip);
		var imported = result.Imported.Should().ContainSingle().Subject;
		imported.Namespace.Should().Be("tlp");
		imported.NumberOfTags.Should().Be(28);
		imported.Status.Should().Be("Success");
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public async Task ImportZipAsync_PartialSuccess207_IsReturnedWithErrors()
	{
		var stub = Stub(
			HttpStatusCode.MultiStatus,
			"""{"imported":[{"namespace":"tlp","numberOfTags":28,"status":"Success"}],"errors":[{"file":"machinetag.json","error":"Invalid JSON format","details":{"line":3},"status":"Failure"},{"file":"other.json","error":"Bad","status":"Failure"}]}""");
		using var client = TestClient.Create(stub);

		var result = await client.Taxonomies.ImportZipAsync(
			new ByteArrayPart([1], "t.zip", "application/zip"),
			TestContext.Current.CancellationToken);

		result.Imported.Should().ContainSingle();
		result.Errors.Should().HaveCount(2);
		result.Errors[0].File.Should().Be("machinetag.json");
		result.Errors[0].Error.Should().Be("Invalid JSON format");
		result.Errors[0].Details!.Value.GetProperty("line").GetInt32().Should().Be(3);
		result.Errors[0].Status.Should().Be("Failure");
		result.Errors[1].Details.Should().BeNull();
	}

	[Fact]
	public async Task ImportZipAsync_NonSeekableStream_IsUploadedOnce()
	{
		var stub = Stub(HttpStatusCode.Created, """{"imported":[]}""");
		using var client = TestClient.Create(stub);
		using var stream = new NonSeekableStream([7, 8, 9]);

		await client.Taxonomies.ImportZipAsync(new StreamPart(stream, "t.zip", "application/zip"), TestContext.Current.CancellationToken);

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Parts.Should().ContainSingle().Which.Bytes.Should().Equal(7, 8, 9);
	}

	[Fact]
	public async Task ImportZipAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Taxonomies.ImportZipAsync(new ByteArrayPart([1], "t.zip", "application/zip"), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}
}
