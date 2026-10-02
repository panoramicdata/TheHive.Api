using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>
/// The mailbox connection settings of an email intake configuration. It models the spec's polymorphic mailbox schemas (the input and
/// output variants of the <c>api</c> and <c>imap</c> kinds, discriminated by <c>_kind</c>) as one flattened class, like <c>Access</c>.
/// Set <see cref="Kind"/> (see <see cref="EmailIntakeMailboxKinds"/>) and the members of that kind.
/// </summary>
/// <remarks>
/// <see cref="Kind"/> is a string, not an enum, and members this class does not model are kept in <see cref="AdditionalProperties"/>, so a
/// mailbox kind that a newer server adds is read and written back unchanged. Credentials (<see cref="EmailIntakeBasicAuth.Password"/>,
/// <see cref="EmailIntakeOAuth2.Secret"/>) only ever travel in a request or response body. The class has no <c>ToString</c> override.
/// </remarks>
public sealed class EmailIntakeMailbox
{
	/// <summary>The mailbox kind: <c>api</c> or <c>imap</c> (see <see cref="EmailIntakeMailboxKinds"/>), or a kind this client does not know. The server requires it.</summary>
	[JsonPropertyName("_kind")]
	public string Kind { get; set; } = string.Empty;

	/// <summary>The provider settings.</summary>
	[JsonPropertyName("provider")]
	public EmailIntakeProvider Provider { get; set; } = new();

	/// <summary>The credentials of the mailbox account.</summary>
	[JsonPropertyName("credential")]
	public EmailIntakeCredential Credential { get; set; } = new();

	/// <summary>The folder emails are fetched from (the server default is <c>Inbox</c>); see <see cref="Interfaces.IEmailIntake.ListFoldersAsync"/>.</summary>
	[JsonPropertyName("inbox")]
	public string? Inbox { get; set; }

	/// <summary>The folder processed emails are moved to; when omitted they are not moved.</summary>
	[JsonPropertyName("archive")]
	public string? Archive { get; set; }

	/// <summary>Whether processed emails are marked as read after fetching (the server default is <see langword="false"/>).</summary>
	[JsonPropertyName("markAsRead")]
	public bool? MarkAsRead { get; set; }

	/// <summary>Members this class does not model (for example those of a mailbox kind added by a newer server), preserved when read and written back.</summary>
	[JsonExtensionData]
	public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
