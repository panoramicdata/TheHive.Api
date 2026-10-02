using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Procedures;

namespace TheHive.Api.Data.Alerts;

/// <summary>The body of a create-alert request (the spec's <c>InputCreateAlert</c>). Unset properties are omitted.</summary>
/// <remarks>This sends the JSON form of the endpoint. The multipart form, which can upload files as observables in the same request, is not modelled: add files afterwards with <c>IAlerts.AddAttachmentsAsync</c>, or reference existing attachments through <see cref="ObservableInput.Attachment"/>.</remarks>
public sealed class AlertCreateRequest
{
	/// <summary>The alert type, which identifies the event category and groups related alerts (1 to 32 characters).</summary>
	[JsonPropertyName("type")]
	public required string Type { get; set; }

	/// <summary>The source system that generated the alert (1 to 32 characters).</summary>
	[JsonPropertyName("source")]
	public required string Source { get; set; }

	/// <summary>The unique reference from the source system (1 to 128 characters); with <see cref="Type"/> and <see cref="Source"/> it identifies the alert.</summary>
	[JsonPropertyName("sourceRef")]
	public required string SourceRef { get; set; }

	/// <summary>The external URL of the alert in the source system.</summary>
	[JsonPropertyName("externalLink")]
	public string? ExternalLink { get; set; }

	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>. The server default is 2.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>When the event that triggered the alert happened. The server default is now.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset? Date { get; set; }

	/// <summary>The tags; unknown tags are created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether to flag the alert.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>. The server default is 2.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>. The server default is 2.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The custom field values to set.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The summary of the triage findings.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The name of an existing alert status. The server default is <c>New</c>.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The login of the user to assign; unassigned when omitted.</summary>
	[JsonPropertyName("assignee")]
	public string? Assignee { get; set; }

	/// <summary>The name of a case template to apply when a case is created from the alert.</summary>
	[JsonPropertyName("caseTemplate")]
	public string? CaseTemplate { get; set; }

	/// <summary>The observables to attach to the alert.</summary>
	[JsonPropertyName("observables")]
	public List<ObservableInput>? Observables { get; set; }

	/// <summary>The tactics, techniques and procedures (TTPs) to link to the alert.</summary>
	[JsonPropertyName("procedures")]
	public List<ProcedureInput>? Procedures { get; set; }
}
