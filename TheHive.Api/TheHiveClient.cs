using Refit;
using TheHive.Api.Interfaces;

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
			// The per-attempt timeout is applied inside AuthRetryHandler so retries and Retry-After waits are not cut short.
			Timeout = System.Threading.Timeout.InfiniteTimeSpan
		};
		Settings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(TheHiveJson.Options),
			// Interface paths are relative (no leading slash) so they append to a path-prefixed BaseUrl such as https://host/thehive/.
			UrlResolution = UrlResolutionMode.Rfc3986,
			ExceptionFactory = response => new ValueTask<Exception?>(TheHiveErrorMapper.CreateAsync(response))
		};
		Cases = RestService.For<ICases>(_httpClient, Settings);
		Alerts = RestService.For<IAlerts>(_httpClient, Settings);
		Comments = RestService.For<IComments>(_httpClient, Settings);
		Tasks = RestService.For<ITasks>(_httpClient, Settings);
		TaskLogs = RestService.For<ITaskLogs>(_httpClient, Settings);
	}

	/// <summary>Case operations.</summary>
	public ICases Cases { get; }

	/// <summary>Alert operations.</summary>
	public IAlerts Alerts { get; }

	/// <summary>Comment operations.</summary>
	public IComments Comments { get; }

	/// <summary>Task operations.</summary>
	public ITasks Tasks { get; }

	/// <summary>Task log operations.</summary>
	public ITaskLogs TaskLogs { get; }

	internal HttpClient HttpClient => _httpClient;

	internal RefitSettings Settings { get; }

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();
}
