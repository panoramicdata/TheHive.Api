using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>A PEM certificate authority certificate trusted for the IMAP server SSL connection (the spec's <c>InputEmailIntakeSSLCert</c> and <c>OutputEmailIntakeSSLCert</c>).</summary>
public sealed class EmailIntakeCertificate
{
	/// <summary>The PEM-encoded certificate data.</summary>
	[JsonPropertyName("data")]
	public string Data { get; set; } = string.Empty;

	/// <summary>The certificate type identifier.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }
}
