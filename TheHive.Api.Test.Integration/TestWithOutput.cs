using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit.Microsoft.DependencyInjection.Abstracts;

namespace TheHive.Api.Test.Integration;

/// <summary>
/// Base class for integration tests. The <see cref="Client"/> is built on first use; if the user-secrets
/// <c>Config</c> is missing or still the example placeholder, the test is skipped rather than failed.
/// </summary>
public abstract class TestWithOutput : TestBed<Fixture>, IDisposable
{
	private readonly TestConfig _config;
	private TheHiveClient? _client;

	protected ILogger Logger { get; }

	protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

	protected TestWithOutput(ITestOutputHelper testOutputHelper, Fixture fixture) : base(testOutputHelper, fixture)
	{
		var loggerFactory = fixture.GetService<ILoggerFactory>(testOutputHelper) ?? throw new InvalidOperationException("LoggerFactory is null");
		Logger = loggerFactory.CreateLogger(GetType());

		_config = fixture.GetService<IOptions<TestConfig>>(testOutputHelper)?.Value
			?? throw new InvalidOperationException("TestConfig is null");
	}

	/// <summary>The client. Skips the running test when Config:BaseAddress or Config:ApiKey is not set.</summary>
	protected TheHiveClient Client => _client ??= CreateClient();

	/// <summary>A unique, recognisable name prefix for anything a test creates on the shared instance.</summary>
	protected static string NewName() => $"[TheHive.Api integration] {Guid.NewGuid()}";

	/// <summary>Runs a cleanup action, ignoring any failure so a cleanup problem never masks the real test outcome.</summary>
	protected static async Task TryCleanupAsync(Func<Task> cleanup)
	{
		try
		{
			await cleanup();
		}
		catch (Exception)
		{
			// Best effort: the entity may already have been deleted by the test.
		}
	}

	private TheHiveClient CreateClient()
	{
		Assert.SkipWhen(
			_config.BaseAddress is null || string.IsNullOrWhiteSpace(_config.ApiKey),
			"TheHive integration tests need user-secrets Config:BaseAddress and Config:ApiKey; see TheHive.Api.Test.Integration/README.md.");

		Assert.SkipWhen(
			_config.BaseAddress!.Host.EndsWith("example.com", StringComparison.OrdinalIgnoreCase),
			"Config:BaseAddress is still the example placeholder; see TheHive.Api.Test.Integration/README.md.");

		return new TheHiveClient(new TheHiveClientOptions
		{
			BaseUrl = _config.BaseAddress.ToString(),
			ApiKey = _config.ApiKey!,
			Organisation = _config.Organisation,
			Logger = Logger
		});
	}

	public new void Dispose()
	{
		_client?.Dispose();
		GC.SuppressFinalize(this);
	}
}
