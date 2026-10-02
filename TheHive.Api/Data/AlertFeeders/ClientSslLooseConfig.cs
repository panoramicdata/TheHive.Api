using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>Loose SSL settings that relax security constraints (the spec's <c>SSLLooseConfig</c>). Not recommended for production.</summary>
public sealed class ClientSslLooseConfig
{
	/// <summary>Accepts any X.509 certificate, including self-signed ones.</summary>
	[JsonPropertyName("acceptAnyCertificate")]
	public bool? AcceptAnyCertificate { get; set; }

	/// <summary>Allows legacy SSL hello messages; the platform default applies when not set.</summary>
	[JsonPropertyName("allowLegacyHelloMessages")]
	public bool? AllowLegacyHelloMessages { get; set; }

	/// <summary>Allows unsafe TLS renegotiation; the platform default applies when not set.</summary>
	[JsonPropertyName("allowUnsafeRenegotiation")]
	public bool? AllowUnsafeRenegotiation { get; set; }

	/// <summary>Turns off hostname verification.</summary>
	[JsonPropertyName("disableHostnameVerification")]
	public bool? DisableHostnameVerification { get; set; }

	/// <summary>Turns off Server Name Indication (SNI).</summary>
	[JsonPropertyName("disableSNI")]
	public bool? DisableSni { get; set; }
}
