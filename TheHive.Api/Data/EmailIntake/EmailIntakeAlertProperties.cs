using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The metadata applied to alerts created from a mailbox (the spec's <c>InputEmailIntakeAlertProperties</c> and <c>OutputEmailIntakeAlertProperties</c>).</summary>
public sealed class EmailIntakeAlertProperties
{
	/// <summary>The type of the alerts created (the server default is <c>email-intake</c>).</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>The source of the alerts created; the server requires it when sending (it defaults to the slugified connector name when the properties are omitted).</summary>
	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	/// <summary>The tags applied to the alerts created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }
}
