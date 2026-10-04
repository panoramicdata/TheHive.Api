using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public partial class AuthRetryHandlerTests
{
	[Theory]
	[InlineData("POST", 503, 2)]
	[InlineData("POST", 429, 2)]
	[InlineData("POST", 500, 1)]
	[InlineData("POST", 502, 1)]
	[InlineData("POST", 504, 1)]
	[InlineData("PATCH", 503, 2)]
	[InlineData("PATCH", 429, 2)]
	[InlineData("PATCH", 500, 1)]
	[InlineData("GET", 500, 2)]
	[InlineData("HEAD", 500, 2)]
	[InlineData("PUT", 504, 2)]
	[InlineData("DELETE", 502, 2)]
	[InlineData("OPTIONS", 500, 2)]
	[InlineData("TRACE", 500, 2)]
	public async Task Send_RetryPolicy_DependsOnVerbAndStatus(string method, int status, int expectedCalls)
	{
		using var harness = new Harness(o => o.MaxRetries = 3);
		harness.Stub.Enqueue((HttpStatusCode)status);
		harness.Stub.Enqueue(HttpStatusCode.OK);
		using var request = new HttpRequestMessage(new HttpMethod(method), "https://hive.test/api/v1/case/~1");
		if (method is "POST" or "PATCH" or "PUT")
		{
			request.Content = new StringContent("""{"title":"t"}""", System.Text.Encoding.UTF8, "application/json");
		}

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Should().HaveCount(expectedCalls);
		response.StatusCode.Should().Be(expectedCalls == 2 ? HttpStatusCode.OK : (HttpStatusCode)status);
		harness.Delays.Should().HaveCount(expectedCalls - 1);
	}

	[Fact]
	public async Task Send_ByteArrayPost503_RetriesWithByteIdenticalBody()
	{
		using var harness = new Harness(o => o.MaxRetries = 3);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);
		byte[] payload = [0, 1, 2, 0xFE, 0xFF];
		using var request = new HttpRequestMessage(HttpMethod.Post, "https://hive.test/api/v1/x") { Content = new ByteArrayContent(payload) };

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Should().HaveCount(2);
		harness.Stub.Calls[0].BodyBytes.Should().Equal(payload);
		harness.Stub.Calls[1].BodyBytes.Should().Equal(payload);
	}

	public static TheoryData<string> ReplayableContentKinds => ["form", "memory", "json"];

	[Theory]
	[MemberData(nameof(ReplayableContentKinds))]
	public async Task Send_OtherReplayableContentPost503_IsRetried(string kind)
	{
		using var harness = new Harness(o => o.MaxRetries = 3);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);
		HttpContent content = kind switch
		{
			"form" => new FormUrlEncodedContent([new("a", "b")]),
			"memory" => new ReadOnlyMemoryContent(new byte[] { 1, 2 }),
			_ => System.Net.Http.Json.JsonContent.Create(new { a = 1 })
		};
		using var request = new HttpRequestMessage(HttpMethod.Post, "https://hive.test/api/v1/x") { Content = content };

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Should().HaveCount(2);
		harness.Stub.Calls[1].BodyBytes.Should().Equal(harness.Stub.Calls[0].BodyBytes);
	}

	public static TheoryData<string> NonReplayableContentKinds => ["stream", "multipart", "custom"];

	[Theory]
	[MemberData(nameof(NonReplayableContentKinds))]
	public async Task Send_NonReplayableContent503_IsNotRetried(string kind)
	{
		using var harness = new Harness(o => o.MaxRetries = 3);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable, """{"type":"Unavailable","message":"busy"}""");
		harness.Stub.Enqueue(HttpStatusCode.OK);
		HttpContent content = kind switch
		{
			"stream" => new StreamContent(new NonSeekableStream([1, 2, 3])),
			"multipart" => new MultipartFormDataContent { { new ByteArrayContent([1]), "attachments", "a.bin" } },
			_ => new CustomContent()
		};
		using var request = new HttpRequestMessage(HttpMethod.Get, "https://hive.test/api/v1/x") { Content = content };

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Should().ContainSingle();
		harness.Delays.Should().BeEmpty();
		response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
	}

	[Fact]
	public async Task ImportAsync_NonSeekableArchive503_MakesOneCallAndThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable, """{"type":"ServiceUnavailable","message":"try later"}""");
		stub.Enqueue(HttpStatusCode.OK, "{}");
		using var client = TestClient.Create(stub, o => o.MaxRetries = 3);
		var archive = new NonSeekableStream([7, 8, 9]);

		var act = () => client.Cases.ImportAsync(
			new Data.Cases.CaseImportRequest { Password = "pw" },
			new Refit.StreamPart(archive, "7.thar", "application/octet-stream"),
			TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.ServiceUnavailable && e.Message == "try later");
		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Parts[1].Bytes.Should().Equal(7, 8, 9);
		archive.TimesDrained.Should().Be(1);
	}

	[Fact]
	public async Task AddAttachmentsAsync_SeekableUpload503_IsNotDuplicated()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable, """{"type":"ServiceUnavailable","message":"try later"}""");
		stub.Enqueue(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub, o => o.MaxRetries = 3);
		using var file = new MemoryStream([1, 2, 3]);

		var act = () => client.Cases.AddAttachmentsAsync("~1", [new Refit.StreamPart(file, "a.txt", "text/plain")], cancellationToken: TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<TheHiveApiException>();
		stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task CreateAsync_RefitJsonBodyPost503_IsRetried()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		stub.Enqueue(HttpStatusCode.Created, """{"_id":"~1"}""");
		using var client = TestClient.Create(stub, o =>
		{
			o.MaxRetries = 1;
			o.RetryBaseDelay = TimeSpan.FromMilliseconds(1);
		});

		var result = await client.Cases.CreateAsync(new Data.Cases.CaseCreateRequest { Title = "t", Description = "d" }, TestContext.Current.CancellationToken);

		result.Id.Should().Be("~1");
		stub.Calls.Should().HaveCount(2);
		stub.Calls[1].Body.Should().Be(stub.Calls[0].Body).And.Be("""{"title":"t","description":"d"}""");
	}

	[Fact]
	public async Task ExportAsync_WithLogger_NeverLogsTheQueryString()
	{
		var logger = new CapturingLogger();
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		stub.EnqueueFile([1], "application/octet-stream", "1.thar");
		using var client = TestClient.Create(stub, o =>
		{
			o.MaxRetries = 1;
			o.RetryBaseDelay = TimeSpan.FromMilliseconds(1);
			o.Logger = logger;
		});

		using var content = await client.Cases.ExportAsync("~1", "fake-export-password", TestContext.Current.CancellationToken);

		stub.Calls.Should().HaveCount(2);
		stub.Calls[0].Uri.Query.Should().Contain("fake-export-password");
		logger.Messages.Should().HaveCount(3);
		logger.Messages.Should().OnlyContain(m => !m.Contains("fake-export-password") && !m.Contains("password"));
		logger.Messages.Should().Contain(m => m.Contains("https://hive.test/api/v1/case/~1/export"));
		logger.Messages.Should().Contain(m => m.Contains("retrying"));
	}

	[Fact]
	public async Task Send_ThreeServerErrorsWithTwoRetries_MakesThreeCallsThenReturnsFinal()
	{
		using var harness = new Harness(o => o.MaxRetries = 2);
		harness.Stub.Enqueue(HttpStatusCode.InternalServerError);
		harness.Stub.Enqueue(HttpStatusCode.BadGateway);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Calls.Should().HaveCount(3);
		harness.Delays.Should().HaveCount(2);
	}
}
