using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The body of an update-module-settings request (the spec's <c>InputEmailIntake</c>); it does not manage individual mailbox configurations.</summary>
public sealed class EmailIntakeModuleUpdateRequest
{
	/// <summary>Whether the module is globally enabled; when off, no emails are fetched from any mailbox (the server default is <see langword="true"/>).</summary>
	[JsonPropertyName("enabled")]
	public bool? Enabled { get; set; }

	/// <summary>The interval between automatic email fetch cycles, in the format <c>&lt;amount&gt; &lt;unit&gt;</c>.</summary>
	[JsonPropertyName("interval")]
	public required string Interval { get; set; }
}
