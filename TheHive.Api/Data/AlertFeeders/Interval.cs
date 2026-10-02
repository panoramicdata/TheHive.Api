using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>A length of time (the spec's <c>Interval</c>): a polling interval or a request timeout.</summary>
public sealed class Interval
{
	/// <summary>The number of time units; it must be greater than 0.</summary>
	[JsonPropertyName("value")]
	public int Value { get; set; }

	/// <summary>The time unit.</summary>
	[JsonPropertyName("unit")]
	public IntervalUnit Unit { get; set; }
}
