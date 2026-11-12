using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>The properties shared by cases and alerts: classification, status and handling timings.</summary>
/// <typeparam name="TStage">The stage enumeration the object's status belongs to.</typeparam>
public abstract class WorkItemEntity<TStage> : TheHiveAuditedEntity
	where TStage : struct, Enum
{
	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int Severity { get; set; }

	/// <summary>The human-readable severity label (for example <c>HIGH</c>).</summary>
	[JsonPropertyName("severityLabel")]
	public string SeverityLabel { get; set; } = string.Empty;

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int Tlp { get; set; }

	/// <summary>The human-readable TLP label (for example <c>AMBER</c>).</summary>
	[JsonPropertyName("tlpLabel")]
	public string TlpLabel { get; set; } = string.Empty;

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int Pap { get; set; }

	/// <summary>The human-readable PAP label (for example <c>AMBER</c>).</summary>
	[JsonPropertyName("papLabel")]
	public string PapLabel { get; set; } = string.Empty;

	/// <summary>The name of the case status. Statuses are configurable, so this is free text (for example <c>New</c> or <c>TruePositive</c>).</summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	/// <summary>The stage derived from <see cref="Status"/>.</summary>
	[JsonPropertyName("stage")]
	public TStage Stage { get; set; }

	/// <summary>The login of the assigned user, if any.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The summary of the investigation findings, if any.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>Extra data populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>When the case first entered a status linked to the <c>New</c> stage.</summary>
	[JsonPropertyName("newDate")]
	public DateTimeOffset NewDate { get; set; }

	/// <summary>When the case first entered a status linked to the <c>InProgress</c> stage, if it has.</summary>
	[JsonPropertyName("inProgressDate")]
	public DateTimeOffset? InProgressDate { get; set; }

	/// <summary>When the case first entered a status linked to the <c>Closed</c> stage, if it has.</summary>
	[JsonPropertyName("closedDate")]
	public DateTimeOffset? ClosedDate { get; set; }

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
