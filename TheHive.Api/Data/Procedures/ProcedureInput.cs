using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Procedures;

/// <summary>A procedure to link, nested in a create request such as <c>AlertCreateRequest.Procedures</c> (the spec's <c>InputProcedure</c>). Unset properties are omitted.</summary>
public sealed class ProcedureInput
{
	/// <summary>The ID of an existing technique from a loaded catalog, for example a MITRE ATT&amp;CK ID such as <c>T1486</c>.</summary>
	[JsonPropertyName("patternId")]
	public required string PatternId { get; set; }

	/// <summary>When the technique occurred during the incident.</summary>
	[JsonPropertyName("occurDate")]
	public required DateTimeOffset OccurDate { get; set; }

	/// <summary>The short name of the tactic; required when the technique belongs to more than one tactic.</summary>
	[JsonPropertyName("tactic")]
	public string? Tactic { get; set; }

	/// <summary>How the technique was carried out in this incident (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }
}
