using TheHive.Api.Data.Admin;
using TheHive.Api.Data.Cases;

namespace TheHive.Api.Test.Core;

public class TransportExceptionTests
{
	private sealed class ScriptedHandler(Func<CancellationToken, Task<HttpResponseMessage>> script) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			_ = request;
			return script(cancellationToken);
		}
	}

	private static async Task CallAsync(Func<CancellationToken, Task<HttpResponseMessage>> script, CancellationToken cancellationToken, int shape, TimeSpan? timeout = null)
	{
		var options = new TheHiveClientOptions { BaseUrl = "https://hive.test/", ApiKey = "fake-key", MaxRetries = 0, Timeout = timeout ?? TimeSpan.FromSeconds(30) };
		using var client = new TheHiveClient(options, new ScriptedHandler(script));
		await (shape switch
		{
			0 => client.Admin.SetLogLevelAsync("root", LogLevels.Off, cancellationToken),
			1 => client.Cases.GetAsync("~1", cancellationToken),
			_ => client.Cases.CreateAsync(new CaseCreateRequest { Title = "t", Description = "d" }, cancellationToken)
		});
	}

	private static async Task<HttpResponseMessage> HangAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
		return new HttpResponseMessage();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	public async Task Timeout_SurfacesAsTimeoutException(int shape)
	{
		var act = () => CallAsync(HangAsync, TestContext.Current.CancellationToken, shape, TimeSpan.FromMilliseconds(50));

		await act.Should().ThrowExactlyAsync<TimeoutException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	public async Task NetworkFailure_SurfacesAsHttpRequestException(int shape)
	{
		var act = () => CallAsync(_ => throw new HttpRequestException("connection refused"), TestContext.Current.CancellationToken, shape);

		(await act.Should().ThrowExactlyAsync<HttpRequestException>()).WithMessage("connection refused");
	}

	[Theory]
	[InlineData(0)]
	[InlineData(1)]
	[InlineData(2)]
	public async Task CallerCancellation_SurfacesAsOperationCanceledException(int shape)
	{
		using var cts = new CancellationTokenSource();
		cts.CancelAfter(TimeSpan.FromMilliseconds(50));

		var act = () => CallAsync(HangAsync, cts.Token, shape);

		(await act.Should().ThrowAsync<OperationCanceledException>()).Which.Should().NotBeOfType<TimeoutException>();
	}

	[Fact]
	public async Task UseAfterDispose_SurfacesAsObjectDisposedException()
	{
		var options = new TheHiveClientOptions { BaseUrl = "https://hive.test/", ApiKey = "fake-key", MaxRetries = 0 };
		var client = new TheHiveClient(options, new ScriptedHandler(_ => Task.FromResult(new HttpResponseMessage())));
		client.Dispose();

		var act = () => client.Admin.SetLogLevelAsync("root", LogLevels.Off, TestContext.Current.CancellationToken);

		await act.Should().ThrowExactlyAsync<ObjectDisposedException>();
	}
}
