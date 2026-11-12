using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Procedures;

/// <summary>A procedure: how an ATT&amp;CK technique was applied in an incident (the spec's <c>OutputProcedure</c>).</summary>
public sealed class Procedure : TheHiveAuditedEntity
{
	/// <summary>How the technique was applied in this incident, if described.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>When the technique occurred during the incident.</summary>
	[JsonPropertyName("occurDate")]
	public DateTimeOffset OccurDate { get; set; }

	/// <summary>The technique ID from the catalog (for example <c>T1486</c>).</summary>
	[JsonPropertyName("patternId")]
	public string? PatternId { get; set; }

	/// <summary>The technique name from the catalog (for example <c>Data Encrypted for Impact</c>).</summary>
	[JsonPropertyName("patternName")]
	public string? PatternName { get; set; }

	/// <summary>The tactic identifier, present when the technique belongs to several tactics (for example <c>impact</c>).</summary>
	[JsonPropertyName("tactic")]
	public string? Tactic { get; set; }

	/// <summary>The human-readable tactic name (for example <c>Impact</c>).</summary>
	[JsonPropertyName("tacticLabel")]
	public string? TacticLabel { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
