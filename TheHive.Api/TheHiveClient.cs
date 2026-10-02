using Refit;

namespace TheHive.Api;

/// <summary>Client for the TheHive 5 REST API.</summary>
public sealed class TheHiveClient : IDisposable
{
	private readonly HttpClient _httpClient;

	/// <summary>Creates a client.</summary>
	/// <param name="options">Connection options.</param>
	public TheHiveClient(TheHiveClientOptions options) : this(options, new HttpClientHandler())
	{
	}

	internal TheHiveClient(TheHiveClientOptions options, HttpMessageHandler inner)
	{
		options.Validate();
		var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
		_httpClient = new HttpClient(new Handlers.AuthRetryHandler(options) { InnerHandler = inner })
		{
			BaseAddress = new Uri(baseUrl),
			Timeout = options.Timeout
		};
		Settings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(TheHiveJson.Options),
			ExceptionFactory = response => new ValueTask<Exception?>(TheHiveErrorMapper.CreateAsync(response))
		};
	}

	internal HttpClient HttpClient => _httpClient;

	internal RefitSettings Settings { get; }

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();
}
