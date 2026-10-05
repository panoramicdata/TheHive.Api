using Refit;
using System.Net;
using System.Text.Json;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Procedures;
using TheHive.Api.Data.Shares;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Data.Timeline;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public partial class CasesTests
{
	[Fact]
	public async Task AddAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);
		using var second = new MemoryStream([4, 5]);

		var result = await client.Cases.AddAttachmentsAsync(
			"~123",
			[new StreamPart(first, "encrypt.ps1", "application/x-powershell"), new StreamPart(second, "notes.txt", "text/plain")],
			new AttachmentUploadOptions { CanRename = true },
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachments");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(3);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "encrypt.ps1" && p.ContentType == "application/x-powershell");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "notes.txt" && p.ContentType == "text/plain");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		call.Parts[2].Name.Should().Be("canRename");
		call.Parts[2].FileName.Should().BeNull();
		call.Parts[2].Text.Should().Be("true");
		AssertFullAttachment(result.Attachments.Should().ContainSingle().Subject);
	}

	[Fact]
	public async Task AddAttachmentsAsync_WithoutCanRename_SendsOnlyFiles()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.Cases.AddAttachmentsAsync(
			"~123",
			[new ByteArrayPart([9], "a.bin")],
			new(), TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("attachments");
		part.FileName.Should().Be("a.bin");
		part.Bytes.Should().Equal(9);
	}

	[Fact]
	public void Attachment_Defaults_AreEmptyNotNull()
	{
		var item = new Attachment();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.Hashes.Should().BeEmpty();
		item.ContentType.Should().BeEmpty();
		item.StorageId.Should().BeEmpty();
		item.Path.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
		new AttachmentUploadResult().Attachments.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAttachmentAsync_PatchesExternalFlag()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.UpdateAttachmentAsync("~123", "~456", new AttachmentUpdateRequest { External = true }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachment/~456");
		stub.Calls[0].Body.Should().Be("""{"external":true}""");
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteAttachmentAsync("~123", "~456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/attachment/~456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task ExportAsync_ReturnsArchiveBytesAndFileName()
	{
		var stub = new StubHandler();
		byte[] archive = [0x50, 0x4B, 0x03, 0x04, 0x00, 0xFF];
		stub.EnqueueFile(archive, "application/octet-stream", "7.thar");
		using var client = TestClient.Create(stub);

		using var content = await client.Cases.ExportAsync("~123", "fake pass", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123/export");
		stub.Calls[0].Uri.Query.Should().Be("?password=fake%20pass");
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("7.thar");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(archive);
	}

	[Fact]
	public async Task ExportAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Your licence does not allow this"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Cases.ExportAsync("~123", "pw", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}

	[Fact]
	public async Task ImportAsync_UploadsJsonAndFilePartsAndMapsResult()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $$"""
			{
				"case":{{FullCaseJson}},
				"observables":[{{MinimalObservableJson}}],
				"procedures":[{
					"_id":"~234567890","_createdAt":1748739600000,"_createdBy":"lucas@example.com","_updatedAt":1776902400000,
					"_updatedBy":"alice@example.com","description":"PowerShell script used to encrypt files.","occurDate":1748739600000,
					"patternId":"T1486","patternName":"Data Encrypted for Impact","tactic":"impact","tacticLabel":"Impact","extraData":{"k":1}
				}],
				"errors":[{"message":"observable skipped"}]
			}
			""");
		using var client = TestClient.Create(stub);
		using var archive = new MemoryStream([7, 8, 9]);
		var request = new CaseImportRequest
		{
			Password = "fake-password",
			SharingParameters = [new ShareSettings { Organisation = "Org" }],
			TaskRule = SharingRule.Manual,
			ObservableRule = SharingRule.AutoShare
		};

		var result = await client.Cases.ImportAsync(request, new StreamPart(archive, "7.thar", "application/octet-stream"), TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/case/import");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Name.Should().Be("_json");
		call.Parts[0].FileName.Should().BeNull();
		call.Parts[0].Text.Should().Be(
			"""{"password":"fake-password","sharingParameters":[{"organisation":"Org"}],"taskRule":"manual","observableRule":"autoShare"}""");
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "file" && p.FileName == "7.thar" && p.ContentType == "application/octet-stream");
		call.Parts[1].Bytes.Should().Equal(7, 8, 9);
		result.Case.Id.Should().Be("~123");
		result.Observables.Should().ContainSingle().Which.Id.Should().Be("~1");
		AssertImportedProcedure(result.Procedures.Should().ContainSingle().Subject);
		result.Errors.Should().ContainSingle().Which.GetProperty("message").GetString().Should().Be("observable skipped");
	}

	private static void AssertImportedProcedure(Procedure procedure)
	{
		procedure.Id.Should().Be("~234567890");
		procedure.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		procedure.CreatedBy.Should().Be("lucas@example.com");
		procedure.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		procedure.UpdatedBy.Should().Be("alice@example.com");
		procedure.Description.Should().Be("PowerShell script used to encrypt files.");
		procedure.OccurDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		procedure.PatternId.Should().Be("T1486");
		procedure.PatternName.Should().Be("Data Encrypted for Impact");
		procedure.Tactic.Should().Be("impact");
		procedure.TacticLabel.Should().Be("Impact");
		procedure.ExtraData["k"].GetInt32().Should().Be(1);
	}

	[Fact]
	public async Task ImportAsync_PasswordOnly_SendsMinimalJson()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, $$"""{"case":{{MinimalCaseJson}}}""");
		using var client = TestClient.Create(stub);

		var result = await client.Cases.ImportAsync(
			new CaseImportRequest { Password = "pw" },
			new ByteArrayPart([1], "a.thar"),
			TestContext.Current.CancellationToken);

		stub.Calls[0].Parts[0].Text.Should().Be("""{"password":"pw"}""");
		result.Observables.Should().BeEmpty();
		result.Procedures.Should().BeEmpty();
		result.Errors.Should().BeEmpty();
	}

	[Fact]
	public void ImportResultAndProcedure_Defaults_AreEmptyNotNull()
	{
		new CaseImportResult().Case.Id.Should().BeEmpty();
		var procedure = new Procedure();
		procedure.Id.Should().BeEmpty();
		procedure.CreatedBy.Should().BeEmpty();
		procedure.ExtraData.Should().BeEmpty();
		procedure.UpdatedBy.Should().BeNull();
		procedure.Description.Should().BeNull();
	}

	private static void AssertFullAttachment(Attachment attachment)
	{
		attachment.Id.Should().Be("~456789012");
		attachment.Type.Should().Be("Attachment");
		attachment.CreatedBy.Should().Be("lucas@example.com");
		attachment.UpdatedBy.Should().Be("alice@example.com");
		attachment.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		attachment.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1776902400000));
		attachment.Name.Should().Be("encrypt.ps1");
		attachment.Hashes.Should().Equal("fake-hash-0001", "fake-hash-0002");
		attachment.Size.Should().Be(2048);
		attachment.ContentType.Should().Be("application/x-powershell");
		attachment.StorageId.Should().Be("fake-storage-id");
		attachment.Path.Should().Be("attachments/fake-storage-id");
		attachment.ExtraData["links"].GetInt32().Should().Be(1);
		attachment.External.Should().BeTrue();
	}
}
