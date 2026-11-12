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

		public void EnqueueTransientThenOk(int failures = 1)
		{
			for (var i = 0; i < failures; i++)
			{
				Stub.Enqueue(HttpStatusCode.ServiceUnavailable);
			}

			Stub.Enqueue(HttpStatusCode.OK);
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

	private static async Task<System.Net.Http.Headers.HttpRequestHeaders> SentHeadersAsync(Action<TheHiveClientOptions>? tweak = null, HttpRequestMessage? request = null)
	{
		using var harness = new Harness(tweak);
		harness.Stub.Enqueue(HttpStatusCode.OK);
		using var message = request ?? new HttpRequestMessage(HttpMethod.Get, "https://hive.test/api/v1/case");

		using var response = await harness.SendAsync(message, TestContext.Current.CancellationToken);

		return harness.Stub.Calls.Single().Headers;
	}

	[Fact]
	public async Task Send_AddsBearerAuthorization()
		=> (await SentHeadersAsync()).Authorization!.ToString().Should().Be("Bearer fake-key");

	[Fact]
	public async Task Send_NoOrganisation_OmitsHeader()
		=> (await SentHeadersAsync()).Contains("X-Organisation").Should().BeFalse();

	[Fact]
	public async Task Send_WithOrganisation_AddsHeader()
		=> (await SentHeadersAsync(o => o.Organisation = "acme")).GetValues("X-Organisation").Should().Equal("acme");

	[Fact]
	public async Task Send_OrganisationAlreadyOnRequest_IsReplaced()
	{
		using var request = new HttpRequestMessage(HttpMethod.Get, "https://hive.test/x");
		request.Headers.Add("X-Organisation", "other");

		var headers = await SentHeadersAsync(o => o.Organisation = "acme", request);

		headers.GetValues("X-Organisation").Should().Equal("acme");
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
		harness.EnqueueTransientThenOk();
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
