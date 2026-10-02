using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>Password credentials for an IMAP mailbox (the spec's <c>InputEmailBasicAuthConfig</c> and <c>OutputEmailIntakeBasicAuth</c>).</summary>
public sealed class EmailIntakeBasicAuth
{
	/// <summary>SECRET. The password of the mailbox account; it only ever travels in a request or response body.</summary>
	[JsonPropertyName("password")]
	public string Password { get; set; } = string.Empty;
}
