using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>A TheHive case (the spec's <c>OutputCase</c>).</summary>
public sealed class Case
{
	/// <summary>The internal identifier (for example <c>~123456789</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Case</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the case.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the case, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the case was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the case was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The incremental case number; usable in place of <see cref="Id"/>.</summary>
	[JsonPropertyName("number")]
	public int Number { get; set; }

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

	/// <summary>When the incident started.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset StartDate { get; set; }

	/// <summary>When the incident ended, if it has.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	/// <summary>Whether the case is flagged.</summary>
	[JsonPropertyName("flag")]
	public bool Flag { get; set; }

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
	public CaseStage Stage { get; set; }

	/// <summary>The summary of the investigation findings, if any.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The impact verdict; only set when the status is <c>TruePositive</c>.</summary>
	[JsonPropertyName("impactStatus")]
	public ImpactStatus? ImpactStatus { get; set; }

	/// <summary>The login of the assigned user, if any.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The access control settings.</summary>
	[JsonPropertyName("access")]
	public Access Access { get; set; } = new() { Kind = AccessKind.Unknown };

	/// <summary>The custom field values.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldValue> CustomFields { get; set; } = [];

	/// <summary>The permissions the current user has on the case.</summary>
	[JsonPropertyName("userPermissions")]
	public List<string> UserPermissions { get; set; } = [];

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

	/// <summary>When the incident occurred according to the earliest linked alert, if any.</summary>
	[JsonPropertyName("alertDate")]
	public DateTimeOffset? AlertDate { get; set; }

	/// <summary>When the earliest linked alert first entered the <c>New</c> stage, if any.</summary>
	[JsonPropertyName("alertNewDate")]
	public DateTimeOffset? AlertNewDate { get; set; }

	/// <summary>When the earliest linked alert first entered the <c>InProgress</c> stage, if any.</summary>
	[JsonPropertyName("alertInProgressDate")]
	public DateTimeOffset? AlertInProgressDate { get; set; }

	/// <summary>When the earliest linked alert was imported into a case, if any.</summary>
	[JsonPropertyName("alertImportedDate")]
	public DateTimeOffset? AlertImportedDate { get; set; }

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

	/// <summary>Time to Resolve (TTR), in milliseconds, if known.</summary>
	[JsonPropertyName("timeToResolve")]
	public long? TimeToResolve { get; set; }

	/// <summary>The handling duration, in milliseconds, if known.</summary>
	[JsonPropertyName("handlingDuration")]
	public long? HandlingDuration { get; set; }
}
