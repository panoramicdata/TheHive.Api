using System.Net;
using System.Net.Http.Headers;
using TheHive.Api.Handlers;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public partial class AuthRetryHandlerTests
{
	private static async Task<List<TimeSpan>> RetryAfterDelaysAsync(RetryConditionHeaderValue retryAfter)
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests, "{}", r => r.Headers.RetryAfter = retryAfter);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		return harness.Delays;
	}

	[Fact]
	public async Task Send_RetryAfterDelta_UsesHeaderValue()
		=> (await RetryAfterDelaysAsync(new RetryConditionHeaderValue(TimeSpan.FromSeconds(7)))).Should().Equal(TimeSpan.FromSeconds(7));

	[Fact]
	public async Task Send_RetryAfterFutureDate_WaitsUntilThen()
	{
		var delays = await RetryAfterDelaysAsync(new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(10)));

		var delay = delays.Should().ContainSingle().Subject;
		delay.Should().BeGreaterThan(TimeSpan.Zero).And.BeLessThanOrEqualTo(TimeSpan.FromSeconds(10));
	}

	[Fact]
	public async Task Send_RetryAfterPastDate_WaitsZero()
		=> (await RetryAfterDelaysAsync(new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddSeconds(-30)))).Should().Equal(TimeSpan.Zero);

	[Fact]
	public async Task Send_HugeRetryAfterDelta_IsCapped()
		=> (await RetryAfterDelaysAsync(new RetryConditionHeaderValue(TimeSpan.FromSeconds(86400)))).Should().Equal(TimeSpan.FromSeconds(30));

	[Fact]
	public async Task Send_HugeRetryAfterDate_IsCapped()
		=> (await RetryAfterDelaysAsync(new RetryConditionHeaderValue(DateTimeOffset.UtcNow.AddDays(1)))).Should().Equal(TimeSpan.FromSeconds(30));

	[Fact]
	public async Task Send_RetryAfterBelowCap_IsUnchanged()
		=> (await RetryAfterDelaysAsync(new RetryConditionHeaderValue(TimeSpan.FromSeconds(29)))).Should().Equal(TimeSpan.FromSeconds(29));

	[Fact]
	public async Task Send_TooManyRequestsWithoutHeader_UsesBackoff()
	{
		using var harness = new Harness(o => o.MaxRetries = 1);
		harness.Stub.Enqueue(HttpStatusCode.TooManyRequests);
		harness.Stub.Enqueue(HttpStatusCode.OK);

		using var response = await harness.GetAsync(TestContext.Current.CancellationToken);

		harness.Delays.Should().Equal(TimeSpan.FromSeconds(2));
	}
}
