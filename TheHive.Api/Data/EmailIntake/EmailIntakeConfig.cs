using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>A mailbox configuration of the email intake module (the spec's <c>OutputEmailIntakeConfig</c>).</summary>
public sealed class EmailIntakeConfig
{
	/// <summary>The unique identifier of the configuration.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The display name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The mailbox connection settings, with credentials as the server returns them.</summary>
	[JsonPropertyName("mailbox")]
	public EmailIntakeMailbox Mailbox { get; set; } = new();

	/// <summary>The organizations where emails from this mailbox create alerts.</summary>
	[JsonPropertyName("organisations")]
	public List<string>? Organisations { get; set; }

	/// <summary>Whether this mailbox configuration is active.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>When the configuration was created.</summary>
	[JsonPropertyName("createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The metadata applied to alerts created from this mailbox.</summary>
	[JsonPropertyName("alertProperties")]
	public EmailIntakeAlertProperties AlertProperties { get; set; } = new();
}
