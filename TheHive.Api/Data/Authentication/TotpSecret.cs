using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Authentication;

/// <summary>
/// A newly generated TOTP secret for setting up multifactor authentication (the spec's <c>OutputTOTPSecret</c>). Both members are secrets: the class does not
/// print them, and they should not be logged or stored beyond the set-up.
/// </summary>
public sealed class TotpSecret
{
	/// <summary>The Base32-encoded secret that generates time-based one-time passwords. SECRET. Send it back in <see cref="TotpActivateRequest.Secret"/> to activate it.</summary>
	[JsonPropertyName("secret")]
	public string Secret { get; set; } = string.Empty;

	/// <summary>The <c>otpauth</c> URI that contains <see cref="Secret"/>, to render as a QR code for an authenticator app. SECRET.</summary>
	[JsonPropertyName("uri")]
	public string Uri { get; set; } = string.Empty;
}
