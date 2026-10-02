using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The HTTP client and proxy settings of an alert feeder request (the spec's <c>ClientProxyWSConfigDto</c>). The server requires <see cref="Ssl"/> and <see cref="Proxy"/>.</summary>
public sealed class ClientProxyConfig
{
	/// <summary>The connection, idle and request timeouts.</summary>
	[JsonPropertyName("timeout")]
	public ClientProxyTimeout? Timeout { get; set; }

	/// <summary>Whether to follow HTTP redirects automatically.</summary>
	[JsonPropertyName("followRedirects")]
	public bool? FollowRedirects { get; set; }

	/// <summary>Whether to use the system proxy properties.</summary>
	[JsonPropertyName("useProxyProperties")]
	public bool? UseProxyProperties { get; set; }

	/// <summary>A custom User-Agent header value (1 to 128 characters).</summary>
	[JsonPropertyName("userAgent")]
	public string? UserAgent { get; set; }

	/// <summary>Whether to enable HTTP response compression.</summary>
	[JsonPropertyName("compressionEnabled")]
	public bool? CompressionEnabled { get; set; }

	/// <summary>The SSL/TLS settings.</summary>
	[JsonPropertyName("ssl")]
	public ClientSslConfig? Ssl { get; set; }

	/// <summary>The maximum number of connections per host; <c>-1</c> for no limit.</summary>
	[JsonPropertyName("maxConnectionsPerHost")]
	public int? MaxConnectionsPerHost { get; set; }

	/// <summary>The maximum total number of connections; <c>-1</c> for no limit.</summary>
	[JsonPropertyName("maxConnectionsTotal")]
	public int? MaxConnectionsTotal { get; set; }

	/// <summary>The maximum lifetime of a pooled connection (a duration string).</summary>
	[JsonPropertyName("maxConnectionLifetime")]
	public string? MaxConnectionLifetime { get; set; }

	/// <summary>How long an idle connection is kept in the pool (a duration string).</summary>
	[JsonPropertyName("idleConnectionInPoolTimeout")]
	public string? IdleConnectionInPoolTimeout { get; set; }

	/// <summary>How often the connection pool cleaner runs (a duration string).</summary>
	[JsonPropertyName("connectionPoolCleanerPeriod")]
	public string? ConnectionPoolCleanerPeriod { get; set; }

	/// <summary>The maximum number of redirects to follow before failing.</summary>
	[JsonPropertyName("maxNumberOfRedirects")]
	public int? MaxNumberOfRedirects { get; set; }

	/// <summary>The maximum number of times a failed request is retried.</summary>
	[JsonPropertyName("maxRequestRetry")]
	public int? MaxRequestRetry { get; set; }

	/// <summary>Whether to skip URL encoding of query parameters.</summary>
	[JsonPropertyName("disableUrlEncoding")]
	public bool? DisableUrlEncoding { get; set; }

	/// <summary>Whether to use HTTP keep-alive connections.</summary>
	[JsonPropertyName("keepAlive")]
	public bool? KeepAlive { get; set; }

	/// <summary>Whether to use a lax cookie encoder that accepts non-standard cookie syntax.</summary>
	[JsonPropertyName("useLaxCookieEncoder")]
	public bool? UseLaxCookieEncoder { get; set; }

	/// <summary>Whether to use a cookie store to persist cookies across requests.</summary>
	[JsonPropertyName("useCookieStore")]
	public bool? UseCookieStore { get; set; }

	/// <summary>The proxy server settings.</summary>
	[JsonPropertyName("proxy")]
	public ClientProxyServer? Proxy { get; set; }
}
