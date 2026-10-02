using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The email intake module configuration (the spec's <c>OutputEmailIntake</c>): the enabled flag, sync interval and all mailbox configurations.</summary>
public sealed class EmailIntakeModule
{
	/// <summary>Whether the module is globally enabled.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>The interval between automatic email fetch cycles, in the format <c>&lt;amount&gt; &lt;unit&gt;</c> (a string in the spec).</summary>
	[JsonPropertyName("interval")]
	public string Interval { get; set; } = string.Empty;

	/// <summary>The mailbox configurations.</summary>
	[JsonPropertyName("configs")]
	public List<EmailIntakeConfig> Configs { get; set; } = [];
}
