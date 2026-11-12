using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The settings of an alert feeder that is identified by name: the create and test request bodies.</summary>
public abstract class AlertFeederNamedSettings : AlertFeederSettings
{
	/// <summary>The name of the alert feeder (1 to 128 characters); it cannot be changed after creation.</summary>
	[JsonPropertyName("name")]
	[JsonPropertyOrder(1)]
	public required string Name { get; set; }
}
