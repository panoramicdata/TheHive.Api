using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public partial class AuthRetryHandlerTests
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
}
