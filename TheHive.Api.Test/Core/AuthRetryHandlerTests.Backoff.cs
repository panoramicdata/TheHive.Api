using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public partial class AuthRetryHandlerTests
{
	private static async Task<List<TimeSpan>> BackoffDelaysAsync(int failures, Action<TheHiveClientOptions> tweak)
	{
		using var harness = new Harness(tweak).EnqueueTransientThenOk(failures);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		return harness.Delays;
	}

	[Fact]
	public async Task Send_TransientThenOk_RetriesOnceWithBaseDelay()
		=> (await BackoffDelaysAsync(1, o => o.MaxRetries = 2)).Should().Equal(TimeSpan.FromSeconds(2));

	[Fact]
	public async Task Send_SecondRetry_DoublesDelay()
		=> (await BackoffDelaysAsync(2, o => o.MaxRetries = 2)).Should().Equal(TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4));

	[Fact]
	public async Task Send_BackoffGrowth_IsCappedAtMaxRetryDelay()
	{
		var delays = await BackoffDelaysAsync(8, o =>
		{
			o.MaxRetries = 8;
			o.MaxRetryDelay = TimeSpan.FromSeconds(10);
		});

		delays.Select(d => d.TotalSeconds).Should().Equal(2, 4, 8, 10, 10, 10, 10, 10);
	}

	[Fact]
	public async Task Send_HugeMaxRetryDelayAndManyRetries_NeverOverflows()
	{
		var delays = await BackoffDelaysAsync(70, o =>
		{
			o.MaxRetries = 70;
			o.RetryBaseDelay = TimeSpan.FromDays(1);
			o.MaxRetryDelay = TimeSpan.MaxValue;
		});

		delays.Should().HaveCount(70).And.OnlyContain(d => d > TimeSpan.Zero);
	}

	[Fact]
	public async Task Send_ZeroBaseDelay_NeverWaits()
	{
		var delays = await BackoffDelaysAsync(2, o =>
		{
			o.MaxRetries = 2;
			o.RetryBaseDelay = TimeSpan.Zero;
		});

		delays.Should().Equal(TimeSpan.Zero, TimeSpan.Zero);
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
}
