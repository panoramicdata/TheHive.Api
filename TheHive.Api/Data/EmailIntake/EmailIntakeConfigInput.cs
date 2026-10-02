using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>
/// A mailbox configuration sent to the server (the spec's <c>InputEmailIntakeConfig</c>). It is the top-level body of add, update, test and
/// list-folders, so it is shared rather than duplicated. It carries the mailbox credentials, which only ever travel in this request body.
/// </summary>
public sealed class EmailIntakeConfigInput
{
	/// <summary>The pre-registered configuration ID for a Google Workspace OAuth 2.0 configuration (from <see cref="Interfaces.IEmailIntake.SetAuthorizationCodeAsync"/>); omit for every other provider.</summary>
	[JsonPropertyName("id")]
	public string? Id { get; set; }

	/// <summary>The display name.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The mailbox connection settings, including the credentials.</summary>
	[JsonPropertyName("mailbox")]
	public required EmailIntakeMailbox Mailbox { get; set; }

	/// <summary>The organizations where incoming emails create alerts; each must match an existing organization.</summary>
	[JsonPropertyName("organisations")]
	public List<string>? Organisations { get; set; }

	/// <summary>Whether this mailbox configuration is active; when off, no emails are fetched (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("enabled")]
	public bool? Enabled { get; set; }

	/// <summary>The creation date; the server sets it when the configuration is saved.</summary>
	[JsonPropertyName("createdAt")]
	public DateTimeOffset? CreatedAt { get; set; }

	/// <summary>The metadata applied to alerts created from this mailbox; when omitted the server uses <c>email-intake</c> as the type, derives the source from the connector name, and sets standard tags.</summary>
	[JsonPropertyName("alertProperties")]
	public EmailIntakeAlertProperties? AlertProperties { get; set; }
}
