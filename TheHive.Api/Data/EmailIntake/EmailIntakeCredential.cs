using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The credentials of a mailbox account (the spec's <c>InputEmailIntakeCredential</c> and <c>OutputEmailIntakeCredential</c>). Set <see cref="BasicAuth"/> for IMAP password authentication or <see cref="OAuth2"/> for OAuth 2.0 providers.</summary>
public sealed class EmailIntakeCredential
{
	/// <summary>The email address or user name of the mailbox account.</summary>
	[JsonPropertyName("email")]
	public string Email { get; set; } = string.Empty;

	/// <summary>The password credentials, for IMAP providers with password authentication.</summary>
	[JsonPropertyName("basicAuth")]
	public EmailIntakeBasicAuth? BasicAuth { get; set; }

	/// <summary>The OAuth 2.0 credentials, for Google Workspace, Microsoft 365 and Microsoft Graph API.</summary>
	[JsonPropertyName("oAuth2")]
	public EmailIntakeOAuth2? OAuth2 { get; set; }
}
