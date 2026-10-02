using Refit;
using System.Net;
using TheHive.Api.Data.TaskLogs;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class TaskLogsTests
{
	private const string FullLogJson = """
		{
			"_id":"~123456","_type":"Log","_createdBy":"alice@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1742048580000,"_updatedAt":1742055780000,
			"message":"Ran memory analysis on CORP-LAPTOP-056 using Volatility.","date":1742048580000,
			"attachments":[{
				"_id":"~456789012","_type":"Attachment","_createdBy":"lucas@example.com","_createdAt":1748739600000,
				"name":"memory.txt","hashes":["fake-hash-0001"],"size":2048,"contentType":"text/plain",
				"id":"fake-storage-id","path":"attachments/fake-storage-id","extraData":{},"external":false
			}],
			"owner":"TheOrganization","includeInTimeline":1742046000000,"extraData":{"links":2}
		}
		""";

	private const string MinimalLogJson = """
		{
			"_id":"~1","_type":"Log","_createdBy":"alice@example.com","_createdAt":1742048580000,
			"message":"m","date":1742048580000,"owner":"TheOrganization","extraData":{}
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
		var stub = Stub(HttpStatusCode.Created, FullLogJson);
		using var client = TestClient.Create(stub);

		var result = await client.TaskLogs.CreateAsync(
			"~84123",
			new TaskLogCreateRequest
			{
				Message = "Ran memory analysis.",
				StartDate = DateTimeOffset.FromUnixTimeMilliseconds(1742048580000),
				IncludeInTimeline = DateTimeOffset.FromUnixTimeMilliseconds(1742046000000)
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/task/~84123/log");
		stub.Calls[0].ContentType.Should().Be("application/json");
		stub.Calls[0].Body.Should().Be("""{"message":"Ran memory analysis.","startDate":1742048580000,"includeInTimeline":1742046000000}""");
		result.Id.Should().Be("~123456");
		result.Type.Should().Be("Log");
		result.CreatedBy.Should().Be("alice@example.com");
		result.UpdatedBy.Should().Be("lucas@example.com");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1742048580000));
		result.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1742055780000));
		result.Message.Should().Be("Ran memory analysis on CORP-LAPTOP-056 using Volatility.");
		result.Date.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1742048580000));
		var attachment = result.Attachments.Should().ContainSingle().Subject;
		attachment.Name.Should().Be("memory.txt");
		attachment.Size.Should().Be(2048);
		result.Owner.Should().Be("TheOrganization");
		result.IncludeInTimeline.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1742046000000));
		result.ExtraData["links"].GetInt32().Should().Be(2);
	}

	[Fact]
	public async Task CreateAsync_MessageOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalLogJson);
		using var client = TestClient.Create(stub);

		var result = await client.TaskLogs.CreateAsync("~1", new TaskLogCreateRequest { Message = "m" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"message":"m"}""");
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Attachments.Should().BeEmpty();
		result.IncludeInTimeline.Should().BeNull();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public void TaskLog_Defaults_AreEmptyNotNull()
	{
		var item = new TaskLog();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Message.Should().BeEmpty();
		item.Owner.Should().BeEmpty();
		item.Attachments.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_PatchesMessageAndPin()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.TaskLogs.UpdateAsync(
			"~123456",
			new TaskLogUpdateRequest { Message = "Edited", IncludeInTimeline = DateTimeOffset.FromUnixTimeMilliseconds(1742046000000) },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/log/~123456");
		stub.Calls[0].Body.Should().Be("""{"message":"Edited","includeInTimeline":1742046000000}""");
	}

	[Fact]
	public async Task UpdateAsync_ExplicitNull_RemovesThePin()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.TaskLogs.UpdateAsync("~123456", new TaskLogUpdateRequest { IncludeInTimeline = null }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"includeInTimeline":null}""");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.TaskLogs.DeleteAsync("~123456", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/log/~123456");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task AddAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);

		await client.TaskLogs.AddAttachmentsAsync(
			"~123456",
			[new StreamPart(first, "memory.txt", "text/plain"), new ByteArrayPart([4, 5], "dump.bin", "application/octet-stream")],
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/log/~123456/attachments");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(2);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "memory.txt" && p.ContentType == "text/plain");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "dump.bin" && p.ContentType == "application/octet-stream");
		call.Parts[1].Bytes.Should().Equal(4, 5);
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.TaskLogs.DeleteAttachmentAsync("~123456", "~456789012", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/log/~123456/attachments/~456789012");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetObservableAttachmentAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] file = [0x4D, 0x5A, 0x00, 0xFF];
		stub.EnqueueFile(file, "application/octet-stream", "sample.exe");
		using var client = TestClient.Create(stub);

		using var content = await client.TaskLogs.GetObservableAttachmentAsync("~8529344", "~456789012", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/observable/~8529344/attachment/~456789012");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Headers.Contains("If-None-Match").Should().BeFalse();
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("sample.exe");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(file);
	}

	[Fact]
	public async Task GetObservableAttachmentAsync_NotModified_SendsIfNoneMatchAndThrows()
	{
		var stub = Stub(HttpStatusCode.NotModified);
		using var client = TestClient.Create(stub);

		var act = () => client.TaskLogs.GetObservableAttachmentAsync("~8529344", "~456789012", "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task CreateAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Task not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.TaskLogs.CreateAsync("~0", new TaskLogCreateRequest { Message = "m" }, TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
