using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Alerts;

/// <summary>A TheHive alert (the spec's <c>OutputAlert</c>).</summary>
public sealed class Alert
{
	/// <summary>The internal identifier (for example <c>~123456789</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Alert</c> (the wire's <c>_type</c>).</summary>
	[JsonPropertyName("_type")]
	public string EntityType { get; set; } = string.Empty;

	/// <summary>The login of the user who created the alert.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the alert, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the alert was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the alert was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The alert type, which identifies the event category and groups related alerts (the wire's <c>type</c>).</summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The source system that generated the alert.</summary>
	[JsonPropertyName("source")]
	public string Source { get; set; } = string.Empty;

	/// <summary>The unique reference from the source system; with <see cref="Type"/> and <see cref="Source"/> it identifies the alert.</summary>
	[JsonPropertyName("sourceRef")]
	public string SourceRef { get; set; } = string.Empty;

	/// <summary>The external URL of the alert in the source system, if any.</summary>
	[JsonPropertyName("externalLink")]
	public string? ExternalLink { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int Severity { get; set; }

	/// <summary>The human-readable severity label.</summary>
	[JsonPropertyName("severityLabel")]
	public string SeverityLabel { get; set; } = string.Empty;

	/// <summary>When the event that triggered the alert happened.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset Date { get; set; }

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int Tlp { get; set; }

	/// <summary>The human-readable TLP label.</summary>
	[JsonPropertyName("tlpLabel")]
	public string TlpLabel { get; set; } = string.Empty;

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int Pap { get; set; }

	/// <summary>The human-readable PAP label.</summary>
	[JsonPropertyName("papLabel")]
	public string PapLabel { get; set; } = string.Empty;

	/// <summary>Whether the alert is followed, so changes in its source (for example a MISP event) update it.</summary>
	[JsonPropertyName("follow")]
	public bool Follow { get; set; }

	/// <summary>The custom field values.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldValue> CustomFields { get; set; } = [];

	/// <summary>The name of the case template used when a case is created from this alert, if any.</summary>
	[JsonPropertyName("caseTemplate")]
	public string? CaseTemplate { get; set; }

	/// <summary>The number of observables on the alert.</summary>
	[JsonPropertyName("observableCount")]
	public long ObservableCount { get; set; }

	/// <summary>The ID of the case created from or linked to this alert, if any.</summary>
	[JsonPropertyName("caseId")]
	public string? CaseId { get; set; }

	/// <summary>The name of the alert status. Statuses are configurable, so this is free text (for example <c>New</c> or <c>Imported</c>).</summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	/// <summary>The stage derived from <see cref="Status"/>.</summary>
	[JsonPropertyName("stage")]
	public AlertStage Stage { get; set; }

	/// <summary>The login of the assigned user, if any.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The summary of the triage findings, if any.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>Extra data populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>When the alert first entered a status linked to the <c>New</c> stage.</summary>
	[JsonPropertyName("newDate")]
	public DateTimeOffset NewDate { get; set; }

	/// <summary>When the alert first entered a status linked to the <c>InProgress</c> stage, if it has.</summary>
	[JsonPropertyName("inProgressDate")]
	public DateTimeOffset? InProgressDate { get; set; }

	/// <summary>When the alert first entered a status linked to the <c>Closed</c> stage, if it has.</summary>
	[JsonPropertyName("closedDate")]
	public DateTimeOffset? ClosedDate { get; set; }

	/// <summary>When the alert first entered the <c>Imported</c> status, if it has.</summary>
	[JsonPropertyName("importedDate")]
	public DateTimeOffset? ImportedDate { get; set; }

	/// <summary>Time to Detect (TTD), in milliseconds.</summary>
	[JsonPropertyName("timeToDetect")]
	public long TimeToDetect { get; set; }

	/// <summary>Time to Triage (TTT), in milliseconds, if known.</summary>
	[JsonPropertyName("timeToTriage")]
	public long? TimeToTriage { get; set; }

	/// <summary>Time to Qualify (TTQ), in milliseconds, if known.</summary>
	[JsonPropertyName("timeToQualify")]
	public long? TimeToQualify { get; set; }

	/// <summary>Time to Acknowledge (TTA), in milliseconds, if known.</summary>
	[JsonPropertyName("timeToAcknowledge")]
	public long? TimeToAcknowledge { get; set; }
}
