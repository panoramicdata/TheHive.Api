using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Procedures;

/// <summary>A procedure: how an ATT&amp;CK technique was applied in an incident (the spec's <c>OutputProcedure</c>).</summary>
public sealed class Procedure
{
	/// <summary>The internal identifier (for example <c>~234567890</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>When the procedure was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who created the procedure.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the procedure was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login of the user who last updated the procedure, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

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
