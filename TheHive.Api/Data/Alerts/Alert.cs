using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Alerts;

/// <summary>A TheHive alert (the spec's <c>OutputAlert</c>).</summary>
public sealed class Alert : WorkItemEntity<AlertStage>
{
	/// <summary>The entity type, always <c>Alert</c> (the wire's <c>_type</c>).</summary>
	[JsonPropertyName("_type")]
	public string EntityType { get; set; } = string.Empty;

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

	/// <summary>When the event that triggered the alert happened.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset Date { get; set; }

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

	/// <summary>When the alert first entered the <c>Imported</c> status, if it has.</summary>
	[JsonPropertyName("importedDate")]
	public DateTimeOffset? ImportedDate { get; set; }

}
