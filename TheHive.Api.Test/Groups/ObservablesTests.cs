using System.Net;
using TheHive.Api.Data.Observables;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ObservablesTests
{
	private const string FullObservableJson = """
		{
			"_id":"~8529344","_type":"Observable","_createdBy":"lucas@example.com","_updatedBy":"alice@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"dataType":"domain","data":"c2.example.test",
			"startDate":1748739600000,"tlp":2,"tlpLabel":"AMBER","pap":3,"papLabel":"RED",
			"tags":["Source IP"],"ioc":true,"sighted":true,"sightedAt":1748822400000,
			"reports":{"VirusTotal_GetReport":{"status":"Success"}},
			"message":"Source IP of the ransomware C2 server","extraData":{"seen":2},"ignoreSimilarity":true,"external":true
		}
		""";

	private const string MinimalObservableJson = """
		{
			"_id":"~1","_type":"Observable","_createdBy":"lucas@example.com","_createdAt":1748739600000,"dataType":"file",
			"startDate":1748739600000,"tlp":2,"tlpLabel":"AMBER","pap":2,"papLabel":"AMBER","ioc":false,"sighted":false,
			"reports":{},"extraData":{},"ignoreSimilarity":false,"external":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullObservable(Observable item)
	{
		item.Id.Should().Be("~8529344");
		item.Type.Should().Be("Observable");
		item.CreatedBy.Should().Be("lucas@example.com");
		item.UpdatedBy.Should().Be("alice@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		item.DataType.Should().Be("domain");
		item.Data.Should().Be("c2.example.test");
		item.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.Attachment.Should().BeNull();
		item.Tlp.Should().Be(2);
		item.TlpLabel.Should().Be("AMBER");
		item.Pap.Should().Be(3);
		item.PapLabel.Should().Be("RED");
		item.Tags.Should().Equal("Source IP");
		item.Ioc.Should().BeTrue();
		item.Sighted.Should().BeTrue();
		item.SightedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748822400000));
		item.Reports["VirusTotal_GetReport"].GetProperty("status").GetString().Should().Be("Success");
		item.Message.Should().Be("Source IP of the ransomware C2 server");
		item.ExtraData["seen"].GetInt32().Should().Be(2);
		item.IgnoreSimilarity.Should().BeTrue();
		item.External.Should().BeTrue();
	}

	[Fact]
	public async Task CreateInCaseAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, $"[{FullObservableJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Observables.CreateInCaseAsync(
			"~354",
			new ObservableInput { DataType = "domain", Data = ["c2.example.test"], Tlp = 2, Ioc = true },
			cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/observable");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Body.Should().Be("""{"dataType":"domain","data":["c2.example.test"],"tlp":2,"ioc":true}""");
		AssertFullObservable(result.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task CreateInCaseAsync_WithDataTypeQuery_SendsIt()
	{
		var stub = Stub(HttpStatusCode.Created, "[]");
		using var client = TestClient.Create(stub);

		var result = await client.Observables.CreateInCaseAsync(
			"7",
			new ObservableInput { DataType = "domain", Data = ["c2.example.test"] },
			"domain",
			TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/7/observable");
		stub.Calls[0].Uri.Query.Should().Be("?dataType=domain");
		result.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateInAlertAsync_PostsBodyAndMapsObservables()
	{
		var stub = Stub(HttpStatusCode.Created, $"[{FullObservableJson},{MinimalObservableJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Observables.CreateInAlertAsync(
			"~354",
			new ObservableInput
			{
				DataType = "file",
				Attachment = [new ObservableAttachmentReference { Name = "a.bin", ContentType = "application/octet-stream", Id = "abc" }],
				IsZip = true,
				ZipPassword = "infected"
			},
			cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/observable");
		stub.Calls[0].Body.Should().Be(
			"""{"dataType":"file","attachment":[{"name":"a.bin","contentType":"application/octet-stream","id":"abc"}],"isZip":true,"zipPassword":"infected"}""");
		result.Should().HaveCount(2);
		AssertFullObservable(result[0]);
		result[1].Id.Should().Be("~1");
	}

	[Fact]
	public async Task CreateInAlertAsync_WithDataTypeQuery_SendsIt()
	{
		var stub = Stub(HttpStatusCode.Created, "[]");
		using var client = TestClient.Create(stub);

		await client.Observables.CreateInAlertAsync(
			"~354",
			new ObservableInput { DataType = "ip" },
			"hostname",
			TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?dataType=hostname");
	}

	[Fact]
	public async Task GetAsync_GetsObservable_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalObservableJson);
		using var client = TestClient.Create(stub);

		var result = await client.Observables.GetAsync("~1", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~1");
		stub.Calls[0].Body.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Data.Should().BeNull();
		result.Attachment.Should().BeNull();
		result.SightedAt.Should().BeNull();
		result.Message.Should().BeNull();
		result.Tags.Should().BeEmpty();
		result.Reports.Should().BeEmpty();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_FullObservable_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullObservableJson);
		using var client = TestClient.Create(stub);

		AssertFullObservable(await client.Observables.GetAsync("~8529344", TestContext.Current.CancellationToken));
	}

	[Fact]
	public async Task UpdateAsync_PatchesOnlySetFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Observables.UpdateAsync("~8529344", new ObservableUpdateRequest { Ioc = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~8529344");
		stub.Calls[0].Body.Should().Be("""{"ioc":true}""");
	}

	[Fact]
	public async Task UpdateAsync_SerializesEveryFieldWithWireNames()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = new ObservableUpdateRequest
		{
			DataType = "domain",
			Message = "m",
			Tlp = 2,
			Pap = 1,
			Tags = ["a"],
			Ioc = true,
			Sighted = false,
			SightedAt = DateTimeOffset.FromUnixTimeMilliseconds(10),
			IgnoreSimilarity = true,
			AddTags = ["b"],
			RemoveTags = ["c"],
			External = false
		};

		await client.Observables.UpdateAsync("~8529344", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be(
			"""{"dataType":"domain","message":"m","tlp":2,"pap":1,"tags":["a"],"ioc":true,"sighted":false,"sightedAt":10,"ignoreSimilarity":true,"addTags":["b"],"removeTags":["c"],"external":false}""");
	}

	[Theory]
	[InlineData("message")]
	[InlineData("sightedAt")]
	public async Task UpdateAsync_ExplicitNull_SendsNullToUnset(string field)
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		var request = field == "message"
			? new ObservableUpdateRequest { Message = null }
			: new ObservableUpdateRequest { SightedAt = null };

		await client.Observables.UpdateAsync("~8529344", request, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be($$"""{"{{field}}":null}""");
	}

	[Fact]
	public async Task BulkUpdateAsync_PatchesIdsFirstThenFields()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Observables.BulkUpdateAsync(
			new ObservableBulkUpdateRequest { Sighted = true, Ids = ["~128458762", "~216513541"], Message = null },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/_bulk");
		stub.Calls[0].Body.Should().Be("""{"ids":["~128458762","~216513541"],"message":null,"sighted":true}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Observables.DeleteAsync("~8529344", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~8529344");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task DownloadAttachmentAsync_ReturnsExactBytes()
	{
		var stub = new StubHandler();
		byte[] file = [0x4D, 0x5A, 0x00, 0xFF];
		stub.EnqueueFile(file, "application/octet-stream", "sample.exe");
		using var client = TestClient.Create(stub);

		using var content = await client.Observables.DownloadAttachmentAsync("~8529344", "~456789012", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~8529344/attachment/~456789012/download");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("sample.exe");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(file);
	}

	[Fact]
	public async Task DownloadAttachmentAsync_AsZip_SendsQueryParameter()
	{
		var stub = new StubHandler();
		byte[] zip = [0x50, 0x4B, 0x03, 0x04];
		stub.EnqueueFile(zip, "application/octet-stream", "sample.zip");
		using var client = TestClient.Create(stub);

		using var content = await client.Observables.DownloadAttachmentAsync("~8529344", "~456789012", asZip: true, cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?asZip=true");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(zip);
	}

	[Fact]
	public async Task DownloadAttachmentAsync_AsZipFalse_SendsLowercaseFalse()
	{
		var stub = new StubHandler();
		stub.EnqueueFile([1], "application/octet-stream", "a.bin");
		using var client = TestClient.Create(stub);

		using var content = await client.Observables.DownloadAttachmentAsync("~1", "~2", asZip: false, cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.Query.Should().Be("?asZip=false");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Observable not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Observables.GetAsync("~0", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
