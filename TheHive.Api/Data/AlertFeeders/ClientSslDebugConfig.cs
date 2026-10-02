using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>SSL debug logging switches (the spec's <c>SSLDebugConfig</c>).</summary>
public sealed class ClientSslDebugConfig
{
	/// <summary>Traces the SSL context, engine, socket factory and key and trust managers.</summary>
	[JsonPropertyName("all")]
	public bool? All { get; set; }

	/// <summary>Traces key manager operations.</summary>
	[JsonPropertyName("keymanager")]
	public bool? KeyManager { get; set; }

	/// <summary>Traces SSL engine and socket factory operations.</summary>
	[JsonPropertyName("ssl")]
	public bool? Ssl { get; set; }

	/// <summary>Traces SSL context operations.</summary>
	[JsonPropertyName("sslctx")]
	public bool? SslContext { get; set; }

	/// <summary>Traces trust manager operations.</summary>
	[JsonPropertyName("trustmanager")]
	public bool? TrustManager { get; set; }
}
