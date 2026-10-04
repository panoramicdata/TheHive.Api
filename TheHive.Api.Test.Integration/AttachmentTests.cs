using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Refit;
using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Observables;
using TheHive.Api.Querying;

namespace TheHive.Api.Test.Integration;

/// <summary>Live checks of the multipart uploads, attachment downloads and the file observable attachment reference.</summary>
public class AttachmentTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task CaseAttachments_UploadFromByteArrayAndNonSeekableStream_ListAndDelete()
	{
		var client = Client;
		string? caseId = null;
		try
		{
			caseId = (await client.Cases.CreateAsync(NewCase(), CancellationToken)).Id;
			var (textName, textBytes) = NewTextFile();
			var (binaryName, binaryBytes) = NewBinaryFile();

			AttachmentUploadResult result;
			using (var stream = new NonSeekableStream(new MemoryStream(binaryBytes)))
			{
				result = await client.Cases.AddAttachmentsAsync(
					caseId,
					[new ByteArrayPart(textBytes, textName, "text/plain"), new StreamPart(stream, binaryName, "application/octet-stream")],
					cancellationToken: CancellationToken);
			}

			result.Attachments.Should().HaveCount(2);
			ShouldDescribe(result.Attachments.Should().ContainSingle(a => a.Name == textName).Which, textName, textBytes, "text/plain");
			ShouldDescribe(result.Attachments.Should().ContainSingle(a => a.Name == binaryName).Which, binaryName, binaryBytes, "application/octet-stream");

			var listed = await ListAttachmentsAsync(QueryBuilder.GetCase(caseId));
			listed.Select(a => a.Id).Should().BeEquivalentTo(result.Attachments.Select(a => a.Id));

			foreach (var attachment in result.Attachments)
			{
				await client.Cases.DeleteAttachmentAsync(caseId, attachment.Id, CancellationToken);
			}

			(await ListAttachmentsAsync(QueryBuilder.GetCase(caseId))).Should().BeEmpty();
		}
		finally
		{
			if (caseId is not null)
			{
				await TryCleanupAsync(() => client.Cases.DeleteAsync(caseId, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task AlertAttachments_UploadListAndDelete()
	{
		var client = Client;
		string? alertId = null;
		try
		{
			alertId = (await client.Alerts.CreateAsync(
				new AlertCreateRequest
				{
					Type = "integration-test",
					Source = "TheHive.Api",
					SourceRef = Guid.NewGuid().ToString("N"),
					Title = NewName(),
					Description = "Created by the TheHive.Api integration tests; safe to delete."
				},
				CancellationToken)).Id;
			var (name, bytes) = NewTextFile();

			var result = await client.Alerts.AddAttachmentsAsync(alertId, [new ByteArrayPart(bytes, name, "text/plain")], cancellationToken: CancellationToken);

			var attachment = result.Attachments.Should().ContainSingle().Which;
			ShouldDescribe(attachment, name, bytes, "text/plain");
			(await ListAttachmentsAsync(QueryBuilder.GetAlert(alertId))).Should().ContainSingle().Which.Id.Should().Be(attachment.Id);

			await client.Alerts.DeleteAttachmentAsync(alertId, attachment.Id, CancellationToken);

			(await ListAttachmentsAsync(QueryBuilder.GetAlert(alertId))).Should().BeEmpty();
		}
		finally
		{
			if (alertId is not null)
			{
				await TryCleanupAsync(() => client.Alerts.DeleteAsync(alertId, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task OrganisationAttachment_GetAndDownload_ReturnExactBytes()
	{
		var client = Client;
		string? attachmentId = null;
		try
		{
			var (name, bytes) = NewTextFile();
			var uploaded = (await client.Organisations.UploadAttachmentsAsync([new ByteArrayPart(bytes, name, "text/plain")], cancellationToken: CancellationToken))
				.Attachments.Should().ContainSingle().Which;
			attachmentId = uploaded.Id;
			ShouldDescribe(uploaded, name, bytes, "text/plain");

			using (var content = await client.Organisations.GetAttachmentAsync(attachmentId, cancellationToken: CancellationToken))
			{
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
				content.Headers.ContentType!.MediaType.Should().Be("text/plain");
			}

			// The server ignores If-None-Match: * (no 304); the ETag itself is a response header, which Task<HttpContent> does not expose.
			using (var content = await client.Organisations.GetAttachmentAsync(attachmentId, "*", CancellationToken))
			{
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
			}

			using (var content = await client.Organisations.DownloadAttachmentAsync(attachmentId, CancellationToken))
			{
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
				content.Headers.ContentDisposition!.FileName.Should().Be(name);
			}

			await client.Organisations.DeleteAttachmentAsync(attachmentId, CancellationToken);
			var deletedId = attachmentId;
			attachmentId = null;

			var act = () => client.Organisations.GetAttachmentAsync(deletedId, cancellationToken: CancellationToken);
			(await act.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}
		finally
		{
			if (attachmentId is not null)
			{
				await TryCleanupAsync(() => client.Organisations.DeleteAttachmentAsync(attachmentId, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task FileObservable_FromOrganisationAttachment_NeedsTheStorageId_AndDownloadsWithAndWithoutZip()
	{
		var client = Client;
		string? caseId = null;
		string? attachmentId = null;
		try
		{
			caseId = (await client.Cases.CreateAsync(NewCase(), CancellationToken)).Id;
			var (name, bytes) = NewTextFile();
			var uploaded = (await client.Organisations.UploadAttachmentsAsync([new ByteArrayPart(bytes, name, "text/plain")], cancellationToken: CancellationToken))
				.Attachments.Should().ContainSingle().Which;
			attachmentId = uploaded.Id;

			// Verified live (TheHive 5.8): the reference id is the storage id (the wire's "id", a hex SHA-256), not the "~..." _id.
			var byEntityId = () => client.Observables.CreateInCaseAsync(caseId, NewFileObservable(uploaded, uploaded.Id), cancellationToken: CancellationToken);
			(await byEntityId.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);

			var observable = (await client.Observables.CreateInCaseAsync(caseId, NewFileObservable(uploaded, uploaded.StorageId), cancellationToken: CancellationToken))
				.Should().ContainSingle().Which;
			observable.DataType.Should().Be("file");
			var observableAttachment = observable.Attachment!;
			observableAttachment.Name.Should().Be(name);
			observableAttachment.StorageId.Should().Be(uploaded.StorageId);
			// The observable gets its own attachment entity holding the same stored file.
			observableAttachment.Id.Should().NotBe(uploaded.Id);

			// asZip omitted and asZip=false both return the file itself.
			foreach (var asZip in new bool?[] { null, false })
			{
				using var content = await client.Observables.DownloadAttachmentAsync(observable.Id, observableAttachment.Id, asZip, CancellationToken);
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
				content.Headers.ContentDisposition!.FileName.Should().Be(name);
			}

			// asZip=true returns a (password-protected) zip archive named after the file.
			using (var zip = await client.Observables.DownloadAttachmentAsync(observable.Id, observableAttachment.Id, true, CancellationToken))
			{
				var zipBytes = await zip.ReadAsByteArrayAsync(CancellationToken);
				zipBytes.Take(4).Should().Equal((byte)'P', (byte)'K', 3, 4);
				// Bit 0 of the first entry's general purpose flags: the entry is encrypted.
				(zipBytes[6] & 1).Should().Be(1);
				zip.Headers.ContentType!.MediaType.Should().Be("application/zip");
				zip.Headers.ContentDisposition!.FileName.Should().Be($"{name}.zip");
			}

			// The download also accepts the storage id in place of the attachment _id.
			using (var content = await client.Observables.DownloadAttachmentAsync(observable.Id, observableAttachment.StorageId, cancellationToken: CancellationToken))
			{
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
			}

			using (var content = await client.TaskLogs.GetObservableAttachmentAsync(observable.Id, observableAttachment.Id, cancellationToken: CancellationToken))
			{
				(await content.ReadAsByteArrayAsync(CancellationToken)).Should().Equal(bytes);
			}

			await client.Observables.DeleteAsync(observable.Id, CancellationToken);
			var getDeleted = () => client.Observables.GetAsync(observable.Id, CancellationToken);
			(await getDeleted.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);

			await client.Organisations.DeleteAttachmentAsync(attachmentId, CancellationToken);
			var deletedId = attachmentId;
			attachmentId = null;
			var getAttachment = () => client.Organisations.GetAttachmentAsync(deletedId, cancellationToken: CancellationToken);
			(await getAttachment.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}
		finally
		{
			if (caseId is not null)
			{
				await TryCleanupAsync(() => client.Cases.DeleteAsync(caseId, CancellationToken.None));
			}

			if (attachmentId is not null)
			{
				await TryCleanupAsync(() => client.Organisations.DeleteAttachmentAsync(attachmentId, CancellationToken.None));
			}
		}
	}

	private static CaseCreateRequest NewCase() => new()
	{
		Title = NewName(),
		Description = "Created by the TheHive.Api integration tests; safe to delete."
	};

	private static ObservableInput NewFileObservable(Attachment uploaded, string referenceId) => new()
	{
		DataType = "file",
		Message = "Created by the TheHive.Api integration tests; safe to delete.",
		Attachment = [new ObservableAttachmentReference { Id = referenceId, Name = uploaded.Name, ContentType = uploaded.ContentType }]
	};

	private static (string Name, byte[] Bytes) NewTextFile()
	{
		var guid = Guid.NewGuid();
		return ($"itest-{guid:N}.txt", Encoding.UTF8.GetBytes($"[TheHive.Api integration] {guid}\n"));
	}

	private static (string Name, byte[] Bytes) NewBinaryFile()
	{
		var guid = Guid.NewGuid();
		var bytes = new byte[300];
		guid.ToByteArray().CopyTo(bytes, 0);
		for (var i = 16; i < bytes.Length; i++)
		{
			bytes[i] = (byte)i;
		}

		return ($"itest-{guid:N}.bin", bytes);
	}

	private static void ShouldDescribe(Attachment attachment, string name, byte[] bytes, string contentType)
	{
		attachment.Id.Should().StartWith("~");
		attachment.Name.Should().Be(name);
		attachment.Size.Should().Be(bytes.Length);
		attachment.ContentType.Should().Be(contentType);
		var sha256 = Convert.ToHexStringLower(SHA256.HashData(bytes));
		attachment.Hashes.Should().Equal(sha256, Convert.ToHexStringLower(SHA1.HashData(bytes)), Convert.ToHexStringLower(MD5.HashData(bytes)));
		attachment.StorageId.Should().Be(sha256);
	}

	private async Task<List<Attachment>> ListAttachmentsAsync(QueryBuilder entity)
	{
		var result = await Client.Query.RunAsync(entity.Related("attachments").Build(), cancellationToken: CancellationToken);
		return result.Deserialize<List<Attachment>>(TheHiveJson.Options) ?? [];
	}
}
