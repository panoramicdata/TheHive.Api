using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>A responder execution, called an action (the spec's <c>OutputAction</c>).</summary>
public sealed class CortexAction
{
	/// <summary>The internal identifier (for example <c>~327696</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Action</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who triggered the action.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the action.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the action was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the action was last updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The ID of the responder that ran.</summary>
	[JsonPropertyName("responderId")]
	public string ResponderId { get; set; } = string.Empty;

	/// <summary>The display name of the responder that ran.</summary>
	[JsonPropertyName("responderName")]
	public string? ResponderName { get; set; }

	/// <summary>
	/// The definition of the responder that ran, as returned by Cortex. The spec declares a string but its example is an object,
	/// so this is kept as raw JSON and reads either form.
	/// </summary>
	[JsonPropertyName("responderDefinition")]
	public JsonElement? ResponderDefinition { get; set; }

	/// <summary>The name of the Cortex server the responder ran on.</summary>
	[JsonPropertyName("cortexId")]
	public string? CortexId { get; set; }

	/// <summary>The ID of the corresponding job on the Cortex server.</summary>
	[JsonPropertyName("cortexJobId")]
	public string? CortexJobId { get; set; }

	/// <summary>The type of the entity the action targeted: <c>Case</c>, <c>Alert</c>, <c>Observable</c>, <c>Task</c> or <c>Log</c>.</summary>
	[JsonPropertyName("objectType")]
	public string ObjectType { get; set; } = string.Empty;

	/// <summary>The ID of the entity the action targeted.</summary>
	[JsonPropertyName("objectId")]
	public string ObjectId { get; set; } = string.Empty;

	/// <summary>The status of the action.</summary>
	[JsonPropertyName("status")]
	public CortexActionStatus Status { get; set; }

	/// <summary>When the action started.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset StartDate { get; set; }

	/// <summary>When the action finished; absent while it is still running.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>
	/// The operations the responder performed, such as adding a tag. The spec declares a JSON-encoded string but its example is
	/// an array, so this is kept as raw JSON and reads either form.
	/// </summary>
	[JsonPropertyName("operations")]
	public JsonElement? Operations { get; set; }

	/// <summary>
	/// The report returned by the responder. The spec declares a JSON-encoded string but its example is an object, so this is kept
	/// as raw JSON and reads either form.
	/// </summary>
	[JsonPropertyName("report")]
	public JsonElement? Report { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
