using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>Proxy server settings (the spec's <c>ProxyConfig</c>). When not set, no proxy is used.</summary>
public sealed class ClientProxyServer
{
	/// <summary>The hostname or IP address of the proxy server (1 to 128 characters).</summary>
	[JsonPropertyName("host")]
	public string? Host { get; set; }

	/// <summary>The port of the proxy server.</summary>
	[JsonPropertyName("port")]
	public int? Port { get; set; }

	/// <summary>Whether the proxy is enabled, turned off, or uses the default system setting. The server requires it.</summary>
	[JsonPropertyName("state")]
	public ClientProxyState? State { get; set; }

	/// <summary>The proxy protocol, <c>http</c> or <c>https</c>.</summary>
	[JsonPropertyName("protocol")]
	public string? Protocol { get; set; }

	/// <summary>The user name to authenticate with the proxy server (1 to 32 characters).</summary>
	[JsonPropertyName("principal")]
	public string? Principal { get; set; }

	/// <summary>SECRET. The password to authenticate with the proxy server (1 to 128 characters); only ever in a request or response body.</summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>The NTLM domain for proxy authentication.</summary>
	[JsonPropertyName("ntlmDomain")]
	public string? NtlmDomain { get; set; }

	/// <summary>The character encoding for proxy authentication.</summary>
	[JsonPropertyName("encoding")]
	public string? Encoding { get; set; }

	/// <summary>The hostnames or patterns that bypass the proxy.</summary>
	[JsonPropertyName("nonProxyHosts")]
	public List<string>? NonProxyHosts { get; set; }
}
