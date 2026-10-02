using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>
/// The provider of a mailbox (the spec's <c>InputEmailIntakeProvider</c>, <c>InputEmailIntakeApiProvider</c>, <c>OutputEmailIntakeImapProvider</c> and
/// <c>OutputEmailIntakeApiProvider</c>, one flattened class). An API provider only has a <see cref="Name"/>; the other members apply to IMAP.
/// </summary>
public sealed class EmailIntakeProvider
{
	/// <summary>
	/// The provider name: <c>imap</c>, <c>office365</c>, <c>google-workspace</c> or <c>MSGraph365</c> (see <see cref="EmailIntakeProviderName"/>).
	/// A string, because the spec's output does not enumerate it and an unrecognised name must survive an update. The server requires it.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The hostname of the IMAP server; required for a custom IMAP provider.</summary>
	[JsonPropertyName("host")]
	public string? Host { get; set; }

	/// <summary>The IMAP protocol variant (the server default is <c>imap</c>).</summary>
	[JsonPropertyName("protocol")]
	public string? Protocol { get; set; }

	/// <summary>The port of the IMAP server (the server default is 993).</summary>
	[JsonPropertyName("port")]
	public int? Port { get; set; }

	/// <summary>Whether SSL is enabled for the IMAP connection (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("ssl")]
	public bool? Ssl { get; set; }

	/// <summary>Whether STARTTLS is enabled for the IMAP connection (the server default is <see langword="false"/>).</summary>
	[JsonPropertyName("startTLS")]
	public bool? StartTls { get; set; }

	/// <summary>Whether the host name is verified against the SSL certificate (the server default is <see langword="true"/>). Do not turn this off in production.</summary>
	[JsonPropertyName("checkServerIdentity")]
	public bool? CheckServerIdentity { get; set; }

	/// <summary>The PEM certificate authority certificates to trust, for a server whose certificate comes from an internal CA or is self-signed.</summary>
	[JsonPropertyName("certificates")]
	public List<EmailIntakeCertificate>? Certificates { get; set; }
}
