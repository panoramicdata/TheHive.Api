using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>An email intake provider available on the server (the spec's <c>OutputRichEmailIntakeProvider</c>).</summary>
public sealed class EmailIntakeProviderInfo
{
	/// <summary>The name of the email provider.</summary>
	[JsonPropertyName("name")]
	public EmailIntakeProviderName Name { get; set; }

	/// <summary>The connection kind: <c>imap</c> for IMAP-based providers, <c>api</c> for API-based providers.</summary>
	[JsonPropertyName("_kind")]
	public EmailIntakeConfigKind Kind { get; set; }
}
