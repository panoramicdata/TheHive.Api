namespace TheHive.Api.Test.Integration;

/// <summary>Integration test settings from the user-secrets section <c>Config</c>. All are optional so that tests skip when absent.</summary>
internal sealed class TestConfig
{
	/// <summary>The TheHive instance URL.</summary>
	public Uri? BaseAddress { get; init; }

	/// <summary>The API key.</summary>
	public string? ApiKey { get; init; }

	/// <summary>The optional organisation name.</summary>
	public string? Organisation { get; init; }
}
