using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public partial class AuthRetryHandlerTests
{
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
			_ = request;
			Calls++;
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		}
	}

	private sealed class ThrowingHandler : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			_ = (request, cancellationToken);
			throw new OperationCanceledException("inner gave up");
		}
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
		{
			_ = context;
			return stream.WriteAsync("custom"u8.ToArray()).AsTask();
		}

		protected override bool TryComputeLength(out long length)
		{
			length = 6;
			return true;
		}
	}
}
