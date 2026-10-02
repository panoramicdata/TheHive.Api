namespace TheHive.Api.Test.Support;

internal static class TestClient
{
	public static TheHiveClient Create(StubHandler stub, Action<TheHiveClientOptions>? tweak = null)
	{
		var options = new TheHiveClientOptions
		{
			BaseUrl = "https://hive.test/",
			ApiKey = "fake-key",
			MaxRetries = 0
		};
		tweak?.Invoke(options);
		return new TheHiveClient(options, stub);
	}
}
