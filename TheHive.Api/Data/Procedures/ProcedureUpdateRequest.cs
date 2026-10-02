using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Procedures;

/// <summary>The body of an update-procedure request (the spec's <c>InputUpdateProcedure</c>). Unset properties are omitted and keep their current value.</summary>
public sealed class ProcedureUpdateRequest
{
	/// <summary>New description of how the technique was carried out (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>New occurrence date of the technique.</summary>
	[JsonPropertyName("occurDate")]
	public DateTimeOffset? OccurDate { get; set; }

	/// <summary>New technique ID from a loaded catalog, for example <c>T1059.001</c>; it must match an existing technique.</summary>
	[JsonPropertyName("patternId")]
	public string? PatternId { get; set; }

	/// <summary>New short name of the tactic; required when the technique belongs to more than one tactic.</summary>
	[JsonPropertyName("tactic")]
	public string? Tactic { get; set; }
}
