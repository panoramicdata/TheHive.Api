using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The response of the get-configs operation (the spec's <c>OutputRichEmailIntake</c>), a wrapper around the module configuration.</summary>
public sealed class EmailIntakeConfigsResult
{
	/// <summary>The email intake module configuration (wire name <c>outputEmailIntake</c>).</summary>
	[JsonPropertyName("outputEmailIntake")]
	public EmailIntakeModule EmailIntake { get; set; } = new();
}
