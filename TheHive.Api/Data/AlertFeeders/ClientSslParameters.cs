using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>SSL engine parameters (the spec's <c>SSLParameters</c>).</summary>
public sealed class ClientSslParameters
{
	/// <summary>The client authentication mode: <c>Need</c> fails the handshake without peer credentials, <c>Want</c> verifies them if provided, <c>Default</c> uses the platform default.</summary>
	[JsonPropertyName("clientAuth")]
	public string? ClientAuth { get; set; }

	/// <summary>The TLS protocol versions to enable for the SSL engine.</summary>
	[JsonPropertyName("protocols")]
	public List<string>? Protocols { get; set; }
}
