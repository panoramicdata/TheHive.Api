using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The body of a create-alert-feeder request (the spec's <c>InputAlertFeeder</c>). Unset properties are omitted. The feeder function must already exist and have the type <c>feeder:alert</c>.</summary>
public sealed class AlertFeederCreateRequest : AlertFeederNamedSettings
{
	/// <summary>The name of the feeder function that converts the HTTP response into alerts; it must reference an existing function of type <c>feeder:alert</c>.</summary>
	[JsonPropertyName("functionName")]
	[JsonPropertyOrder(6)]
	public required string FunctionName { get; set; }
}
