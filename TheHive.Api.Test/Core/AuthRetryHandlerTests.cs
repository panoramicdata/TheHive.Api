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

		public Harness(Action<TheHiveClientOptions>? tweak = null, bool recordDelays = true)
		{
			var options = new TheHiveClientOptions
			{
				BaseUrl = "https://hive.test/",
				ApiKey = "secret-key",
				MaxRetries = 0,
				RetryBaseDelay = TimeSpan.FromSeconds(2)
			};
			tweak?.Invoke(options);
			Handler = new AuthRetryHandler(options) { InnerHandler = Stub };
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

		harness.Stub.Calls.Single().Headers.Authorization!.ToString().Should().Be("Bearer secret-key");
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
		logger.Messages.Should().OnlyContain(m => !m.Contains("secret-key"));
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
}
