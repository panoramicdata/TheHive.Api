using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>
/// A responder to run on one entity (the spec's <c>InputAction</c>). It is the top-level body of both the single and the bulk run
/// operations, so it is shared rather than duplicated.
/// </summary>
public sealed class CortexActionInput
{
	/// <summary>The ID of the responder to run; it must be available for the target entity.</summary>
	[JsonPropertyName("responderId")]
	public required string ResponderId { get; set; }

	/// <summary>The name of the Cortex server to run the responder on; omit to let TheHive pick the first server that offers it.</summary>
	[JsonPropertyName("cortexId")]
	public string? CortexId { get; set; }

	/// <summary>The type of entity the responder runs on: <c>case</c>, <c>case_artifact</c>, <c>case_task</c>, <c>case_task_log</c> or <c>alert</c>.</summary>
	[JsonPropertyName("objectType")]
	public required string ObjectType { get; set; }

	/// <summary>The ID of the entity (preceded by <c>~</c>), or another identifier its type accepts, such as the case number.</summary>
	[JsonPropertyName("objectId")]
	public required string ObjectId { get; set; }

	/// <summary>Extra parameters passed to the responder; the keys are defined by the responder's own configuration on Cortex.</summary>
	[JsonPropertyName("parameters")]
	public Dictionary<string, object?>? Parameters { get; set; }

	/// <summary>The TLP level of the target entity (0 to 4, see <c>Tlp</c>); the responder runs only if its own maximum TLP is at least this value.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }
}
