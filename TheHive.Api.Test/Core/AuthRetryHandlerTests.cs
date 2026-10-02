using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public class AuthRetryHandlerTests
{
	private sealed class Harness : IDisposable
	{
		private readonly HttpMessageInvoker _invoker;

		public Harness(Action<TheHiveClientOptions>? tweak = null, bool recordDelays = true, HttpMessageHandler? inner = null)
		{
			var options = new TheHiveClientOptions
			{
				BaseUrl = "https://hive.test/",
				ApiKey = "fake-key",
				MaxRetries = 0,
				RetryBaseDelay = TimeSpan.FromSeconds(2)
			};
			tweak?.Invoke(options);
			Handler = new AuthRetryHandler(options) { InnerHandler = inner ?? Stub };
			if (recordDelays)
			{
				Handler.Delay = (delay, _) =>
				{
					Delays.Add(delay);
					return Task.CompletedTask;
				};
			}

			_invoker = new HttpMessageInvoker(Handler);
		}

		public StubHandler Stub { get; } = new();

		public AuthRetryHandler Handler { get; }

		public List<TimeSpan> Delays { get; } = [];

		public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> _invoker.SendAsync(request, cancellationToken);

		public Task<HttpResponseMessage> GetAsync(CancellationToken cancellationToken)
			=> SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://hive.test/api/v1/case"), cancellationToken);

		public void Dispose() => _invoker.Dispose();
	}

	[Fact]
	public async Task Send_AddsBearerAuthorization()
	{
		using var harness = new Harness();
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Stub.Calls.Single().Headers.Authorization!.ToString().Should().Be("Bearer fake-key");
	}

	[Fact]
	public async Task Send_NoOrganisation_OmitsHeader()
	{
		using var harness = new Harness();
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Stub.Calls.Single().Headers.Contains("X-Organisation").Should().BeFalse();
	}

	[Fact]
	public async Task Send_WithOrganisation_AddsHeader()
	{
		using var harness = new Harness(o => o.Organisation = "acme");
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Stub.Calls.Single().Headers.GetValues("X-Organisation").Should().Equal("acme");
	}

	[Fact]
	public async Task Send_OrganisationAlreadyOnRequest_IsReplaced()
	{
		using var harness = new Harness(o => o.Organisation = "acme");
		harness.Stub.Enqueue(HttpStatusCode.OK);
		using var request = new HttpRequestMessage(HttpMethod.Get, "https://hive.test/x");
		request.Headers.Add("X-Organisation", "other");

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Single().Headers.GetValues("X-Organisation").Should().Equal("acme");
	}

	[Fact]
	public async Task Send_TransientThenOk_RetriesOnceWithBaseDelay()
	{
		using var harness = new Harness(o => o.MaxRetries = 2);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Should().HaveCount(2);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Send_SecondRetry_DoublesDelay()
	{
		using var harness = new Harness(o => o.MaxRetries = 2);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.BadGateway);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));
	}

	[Fact]
	public async Task Send_RetryAfterDelta_UsesHeaderValue()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(7)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(7));
	}

	[Fact]
	public async Task Send_RetryAfterFutureDate_WaitsUntilThen()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(10)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		var delay = harness.Delays.Should().ContainSingle().Subject;
		delay.Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThanOrEqualTo(TimeSpan.FromSeconds(10));
	}

	[Fact]
	public async Task Send_RetryAfterPastDate_WaitsZero()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(-30)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.Zero);
	}

	[Fact]
	public async Task Send_BackoffGrowth_IsCappedAtMaxRetryDelay()
	{
		using var harness = new Harness(o =>
		{
			o.MaxRetries = 8;
			o.RetryBaseDelay = TimeSpan.FromSeconds(2);
			o.MaxRetryDelay = TimeSpan.FromSeconds(10);
		});
		for (var i = 0; i < 8; i++)
		{
			harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		}

		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Select(d => d.TotalSeconds).Should().Equal(2, 4, 8, 10, 10, 10, 10, 10);
	}

	[Fact]
	public async Task Send_HugeMaxRetryDelayAndManyRetries_NeverOverflows()
	{
		using var harness = new Harness(o =>
		{
			o.MaxRetries = 70;
			o.RetryBaseDelay = TimeSpan.FromDays(1);
			o.MaxRetryDelay = TimeSpan.MaxValue;
		});
		for (var i = 0; i < 70; i++)
		{
			harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		}

		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().HaveCount(70).And.OnlyContain(d => d > TimeSpan.Zero);
	}

	[Fact]
	public async Task Send_ZeroBaseDelay_NeverWaits()
	{
		using var harness = new Harness(o =>
		{
			o.MaxRetries = 2;
			o.RetryBaseDelay = TimeSpan.Zero;
		});
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.Zero, TimeSpan.Zero);
	}

	[Fact]
	public async Task Send_HugeRetryAfterDelta_IsCapped()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(86400)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(30));
	}

	[Fact]
	public async Task Send_HugeRetryAfterDate_IsCapped()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddDays(1)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(30));
	}

	[Fact]
	public async Task Send_RetryAfterBelowCap_IsUnchanged()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(29)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(29));
	}

	[Fact]
	public async Task Send_MutatingOptionsAfterConstruction_DoesNotChangeHeaders()
	{
		var options = new TheHiveClientOptions { BaseUrl = "https://hive.test/", ApiKey = "fake-key", Organisation = "org-a", MaxRetries = 0 };
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var invoker = new HttpMessageInvoker(new AuthRetryHandler(options) { InnerHandler = stub });

		options.ApiKey = "other-key";
		options.Organisation = "org-b";
		using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://hive.test/api/v1/case"), TestContext.Current.CancellationToken);

		var call = stub.Calls.Single();
		call.Headers.Authorization!.ToString().Should().Be("Bearer fake-key");
		call.Headers.GetValues("X-Organisation").Should().Equal("org-a");
	}

	[Fact]
	public async Task Send_TooManyRequestsWithoutHeader_UsesBackoff()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(2));
	}

	[Fact]
	public async Task Send_PersistentServerError_ReturnsFinalResponseAfterRetriesExhausted()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.InternalServerError);
		harness.Stub.Enqueue(HttpStatusCode.InternalServerError, """{"type":"Boom","message":"still broken"}""");

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
		harness.Stub.Calls.Should().HaveCount(2);
		harness.Delays.Should().HaveCount(1);
		var exception = await TheHiveErrorMapper.CreateAsync(response);
		exception.Should().BeOfType<TheHiveApiException>().Which.Message.Should().Be("still broken");
	}

	[Theory]
	[InlineData(HttpStatusCode.BadRequest)]
	[InlineData(HttpStatusCode.NotFound)]
	public async Task Send_ClientError_IsNotRetried(HttpStatusCode status)
	{
		using var harness = new Harness(o => o.MaxRetries = 3);
		harness.Stub.Enqueue(status);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(status);
		harness.Stub.Calls.Should().ContainSingle();
		harness.Delays.Should().BeEmpty();
	}

	[Fact]
	public async Task Send_RetriedPost_ResendsBody()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);
		using var request = new HttpRequestMessage(HttpMethod.Post, "https://hive.test/api/v1/case")
		{
			Content = new StringContent("""{"title":"t"}""", System.Text.Encoding.UTF8, "application/json")
		};

		using var response = await harness.SendAsync(request, TestContext.Current.CancellationToken);

		harness.Stub.Calls.Should().HaveCount(2);
		harness.Stub.Calls.Select(c => c.Body).Should().AllBe("""{"title":"t"}""");
	}

	[Fact]
	public async Task Send_DelayThrowsCancellation_Propagates()
	{
		using var harness = new Harness(o => o.MaxRetries = 2);
		harness.Handler.Delay = (_, _) => throw new OperationCanceledException();
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);

		var act = () => harness.GetAsync(TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<OperationCanceledException>();
		harness.Stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task Send_DefaultDelay_RetriesWithRealDelay()
	{
		using var harness = new Harness(
			o =>
			{
				o.MaxRetries = 1;
				o.RetryBaseDelay = TimeSpan.FromMilliseconds(1);
			},
			recordDelays: false);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Stub.Calls.Should().HaveCount(2);
	}

	[Fact]
	public async Task Send_DefaultDelay_PreCancelledToken_PropagatesPromptly()
	{
		using var harness = new Harness(
			o =>
			{
				o.MaxRetries = 3;
				o.RetryBaseDelay = TimeSpan.FromMinutes(5);
			},
			recordDelays: false);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		using var cts = new CancellationTokenSource();
		await cts.CancelAsync();

		var act = () => harness.GetAsync(cts.Token);

		await act.Should().ThrowAsync<OperationCanceledException>();
		harness.Stub.Calls.Should().ContainSingle();
	}

	[Fact]
	public async Task Send_WithLogger_LogsWithoutLeakingApiKey()
	{
		var logger = new CapturingLogger();
		using var harness = new Harness(o =>
		{
			o.MaxRetries = 1;
			o.Logger = logger;
		});
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		logger.Messages.Should().NotBeEmpty();
		logger.Messages.Should().OnlyContain(m => !m.Contains("fake-key"));
		logger.Messages.Should().Contain(m => m.Contains("retrying"));
	}

	[Fact]
	public async Task Send_NullLogger_DoesNotThrow()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		var act = async () =>
		{
			using var response = await harness.GetAsync(TestContext.Current.CancellationToken);
		};

		await act.Should().NotThrowAsync();
	}

	private sealed class HangingHandler : HttpMessageHandler
	{
		public int Calls { get; private set; }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			Calls++;
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}

	private sealed class ThrowingHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
			=> throw new OperationCanceledException("inner gave up");
	}

	[Fact]
	public async Task Send_AttemptExceedsTimeout_ThrowsTimeoutException()
	{
		var hanging = new HangingHandler();
		using var harness = new Harness(o => o.Timeout = TimeSpan.FromMilliseconds(50), inner: hanging);

		var act = () => harness.GetAsync(TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<TimeoutException>();
		hanging.Calls.Should().Be(1);
	}

	[Fact]
	public async Task Send_RetryAfterLongerThanTimeout_StillRetriesAndSucceeds()
	{
		using var harness = new Harness(o =>
		{
			o.MaxRetries = 1;
			o.Timeout = TimeSpan.FromMilliseconds(50);
			o.MaxRetryDelay = TimeSpan.FromMinutes(10);
		});
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(120)));
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		response.StatusCode.Should().Be(HttpStatusCode.OK);
		harness.Delays.Should().Equal(TimeSpan.FromSeconds(120));
	}

	[Fact]
	public async Task Send_CallerCancellationDuringAttempt_IsNotATimeout()
	{
		var hanging = new HangingHandler();
		using var harness = new Harness(o => o.Timeout = TimeSpan.FromMinutes(5), inner: hanging);
		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromMilliseconds(50));

		var act = () => harness.GetAsync(cts.Token);

		var thrown = (await act.Should().ThrowAsync<OperationCanceledException>()).Which;
		thrown.Should().NotBeOfType<TimeoutException>();
		cts.IsCancellationRequested.Should().BeTrue();
	}

	[Fact]
	public async Task Send_InnerThrowsCancellationUnprompted_Propagates()
	{
		using var harness = new Harness(inner: new ThrowingHandler());

		var act = () => harness.GetAsync(TestContext.Current.CancellationToken);

		await act.Should().ThrowAsync<OperationCanceledException>().WithMessage("inner gave up");
	}

	private sealed class CustomContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context)
			=> stream.WriteAsync("custom"u8.ToArray()).AsTask();

		protected override bool TryComputeLength(out long length)
		{
			length = 6;
			return true;
		}
	}

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
