using Refit;
using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Procedures;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class AlertsTests
{
	[Fact]
	public async Task MergeIntoCaseAsync_PostsAndMapsCase()
	{
		var stub = Stub(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.MergeIntoCaseAsync("~354", "~123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/merge/~123");
		stub.Calls[0].Body.Should().BeNull();
		result.Id.Should().Be("~123");
		result.Number.Should().Be(7);
	}

	[Fact]
	public async Task ImportIntoCaseAsync_PostsAndMapsCase()
	{
		var stub = Stub(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.ImportIntoCaseAsync("~354", "7", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/import/7");
		stub.Calls[0].Body.Should().BeNull();
		result.Id.Should().Be("~123");
	}

	[Fact]
	public async Task BulkMergeIntoCaseAsync_PostsCaseAndAlertIds()
	{
		var stub = Stub(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.BulkMergeIntoCaseAsync(
			new AlertBulkMergeRequest { CaseId = "~216513541", AlertIds = ["~1", "~2"] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/merge/_bulk");
		stub.Calls[0].Body.Should().Be("""{"caseId":"~216513541","alertIds":["~1","~2"]}""");
		result.Number.Should().Be(7);
	}

	[Fact]
	public async Task BulkMergeIntoCaseAsync_CaseOnly_OmitsAlertIds()
	{
		var stub = Stub(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		await client.Alerts.BulkMergeIntoCaseAsync(new AlertBulkMergeRequest { CaseId = "7" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"caseId":"7"}""");
	}

	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public async Task FollowAndUnfollowAsync_PostWithoutBody(bool follow)
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		if (follow)
		{
			await client.Alerts.FollowAsync("~354", TestContext.Current.CancellationToken);
		}
		else
		{
			await client.Alerts.UnfollowAsync("~354", TestContext.Current.CancellationToken);
		}

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be(follow ? "/api/v1/alert/~354/follow" : "/api/v1/alert/~354/unfollow");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetSimilarObservablesAsync_MapsObservables()
	{
		var stub = Stub(HttpStatusCode.OK, $$$"""
			[{"_id":"~8529344","_type":"Observable","_createdBy":"lucas@example.com","_updatedBy":"alice@example.com",
			"_createdAt":1748739600000,"_updatedAt":1776902400000,"dataType":"file","data":null,
			"startDate":1748739600000,"attachment":{{{AttachmentJson}}},"tlp":2,"tlpLabel":"AMBER","pap":3,"papLabel":"RED",
			"tags":["Source IP"],"ioc":true,"sighted":true,"sightedAt":1748822400000,
			"reports":{"VirusTotal_GetReport":{"status":"Success"}},"message":"m","extraData":{"seen":2},"ignoreSimilarity":true,"external":true}]
			""");
		using var client = TestClient.Create(stub);

		var result = await client.Alerts.GetSimilarObservablesAsync("~354", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/similar/~456/observables");
		var item = result.Should().ContainSingle().Subject;
		item.Id.Should().Be("~8529344");
		item.Type.Should().Be("Observable");
		item.CreatedBy.Should().Be("lucas@example.com");
		item.UpdatedBy.Should().Be("alice@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		item.DataType.Should().Be("file");
		item.Data.Should().BeNull();
		item.StartDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.Attachment!.Name.Should().Be("encrypt.ps1");
		item.Tlp.Should().Be(Tlp.Amber);
		item.TlpLabel.Should().Be("AMBER");
		item.Pap.Should().Be(Pap.Red);
		item.PapLabel.Should().Be("RED");
		item.Tags.Should().Equal("Source IP");
		item.Ioc.Should().BeTrue();
		item.Sighted.Should().BeTrue();
		item.SightedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748822400000));
		item.Message.Should().Be("m");
		item.Reports["VirusTotal_GetReport"].GetProperty("status").GetString().Should().Be("Success");
		item.ExtraData["seen"].GetInt32().Should().Be(2);
		item.IgnoreSimilarity.Should().BeTrue();
		item.External.Should().BeTrue();
	}

	[Fact]
	public async Task AddAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart()
	{
		var stub = Stub(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);
		using var second = new MemoryStream([4, 5]);

		var result = await client.Alerts.AddAttachmentsAsync(
			"~354",
			[new StreamPart(first, "encrypt.ps1", "application/x-powershell"), new StreamPart(second, "notes.txt", "text/plain")],
			new AttachmentUploadOptions { CanRename = true },
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/attachments");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(3);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "encrypt.ps1" && p.ContentType == "application/x-powershell");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "notes.txt" && p.ContentType == "text/plain");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		call.Parts[2].Name.Should().Be("canRename");
		call.Parts[2].FileName.Should().BeNull();
		call.Parts[2].Text.Should().Be("true");
		var attachment = result.Attachments.Should().ContainSingle().Subject;
		attachment.Id.Should().Be("~456789012");
		attachment.Name.Should().Be("encrypt.ps1");
		attachment.Size.Should().Be(2048);
		attachment.StorageId.Should().Be("fake-storage-id");
	}

	[Fact]
	public async Task AddAttachmentsAsync_WithoutCanRename_SendsOnlyFiles()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Alerts.AddAttachmentsAsync(
			"~354",
			[new ByteArrayPart([9], "a.bin")],
			new(), TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("attachments");
		part.FileName.Should().Be("a.bin");
		part.Bytes.Should().Equal(9);
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Alerts.DeleteAttachmentAsync("~354", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/attachment/~456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Alert not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Alerts.GetAsync("~missing", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e =>
				e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError" && e.Message == "Alert not found");
	}
}
