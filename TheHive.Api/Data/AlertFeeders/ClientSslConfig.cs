using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>SSL/TLS settings of the HTTP client (the spec's <c>SSLConfig</c>).</summary>
public sealed class ClientSslConfig
{
	/// <summary>Whether to use the default JVM SSL configuration.</summary>
	[JsonPropertyName("default")]
	public bool? Default { get; set; }

	/// <summary>The SSL/TLS protocol version to use, for example <c>TLSv1.2</c>.</summary>
	[JsonPropertyName("protocol")]
	public string? Protocol { get; set; }

	/// <summary>Whether to check certificate revocation lists.</summary>
	[JsonPropertyName("checkRevocation")]
	public bool? CheckRevocation { get; set; }

	/// <summary>The certificate revocation list URLs to check.</summary>
	[JsonPropertyName("revocationLists")]
	public List<string>? RevocationLists { get; set; }

	/// <summary>The cipher suites that override the platform default.</summary>
	[JsonPropertyName("enabledCipherSuites")]
	public List<string>? EnabledCipherSuites { get; set; }

	/// <summary>The TLS protocols that override the platform default.</summary>
	[JsonPropertyName("enabledProtocols")]
	public List<string>? EnabledProtocols { get; set; }

	/// <summary>The hostname verifier class name.</summary>
	[JsonPropertyName("hostnameVerifierClass")]
	public string? HostnameVerifierClass { get; set; }

	/// <summary>The SecureRandom implementation class name.</summary>
	[JsonPropertyName("secureRandom")]
	public string? SecureRandom { get; set; }

	/// <summary>The trust manager settings.</summary>
	[JsonPropertyName("trustManager")]
	public ClientKeyManager? TrustManager { get; set; }

	/// <summary>The key manager settings.</summary>
	[JsonPropertyName("keyManager")]
	public ClientKeyManager? KeyManager { get; set; }

	/// <summary>The SSL engine parameters.</summary>
	[JsonPropertyName("sslParametersConfig")]
	public ClientSslParameters? SslParametersConfig { get; set; }

	/// <summary>The SSL debug logging switches.</summary>
	[JsonPropertyName("debug")]
	public ClientSslDebugConfig? Debug { get; set; }

	/// <summary>The loose settings. The server requires them.</summary>
	[JsonPropertyName("loose")]
	public ClientSslLooseConfig? Loose { get; set; }
}
