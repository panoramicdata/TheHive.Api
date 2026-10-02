using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>
/// A length of time (the spec's <c>Interval</c>): a polling interval or a request timeout. <see cref="Unit"/> is a tolerant enum: a unit this
/// client does not know reads as <see cref="IntervalUnit.Unknown"/> and is written back as <c>Unknown</c>, so do not round-trip a read model
/// blindly into an update.
/// </summary>
public sealed class Interval
{
	/// <summary>The number of time units; it must be greater than 0.</summary>
	[JsonPropertyName("value")]
	public int Value { get; set; }

	/// <summary>The time unit.</summary>
	[JsonPropertyName("unit")]
	public IntervalUnit Unit { get; set; }
}
