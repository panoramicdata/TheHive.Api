using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Procedures;

namespace TheHive.Api.Data.Alerts;

/// <summary>The body of a create-alert request (the spec's <c>InputCreateAlert</c>). Unset properties are omitted.</summary>
/// <remarks>This sends the JSON form of the endpoint. The multipart form, which can upload files as observables in the same request, is not modelled: add files afterwards with <c>IAlerts.AddAttachmentsAsync</c>, or reference existing attachments through <see cref="ObservableInput.Attachment"/>.</remarks>
public sealed class AlertCreateRequest : ClassifiedRequestBase
{
	/// <summary>The alert type, which identifies the event category and groups related alerts (1 to 32 characters).</summary>
	[JsonPropertyName("type")]
	[JsonPropertyOrder(1)]
	public required string Type { get; set; }

	/// <summary>The source system that generated the alert (1 to 32 characters).</summary>
	[JsonPropertyName("source")]
	[JsonPropertyOrder(2)]
	public required string Source { get; set; }

	/// <summary>The unique reference from the source system (1 to 128 characters); with <see cref="Type"/> and <see cref="Source"/> it identifies the alert.</summary>
	[JsonPropertyName("sourceRef")]
	[JsonPropertyOrder(3)]
	public required string SourceRef { get; set; }

	/// <summary>The external URL of the alert in the source system.</summary>
	[JsonPropertyName("externalLink")]
	[JsonPropertyOrder(4)]
	public string? ExternalLink { get; set; }

	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	[JsonPropertyOrder(5)]
	public required string Title { get; set; }

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	[JsonPropertyOrder(6)]
	public required string Description { get; set; }

	/// <summary>When the event that triggered the alert happened. The server default is now.</summary>
	[JsonPropertyName("date")]
	[JsonPropertyOrder(110)]
	public DateTimeOffset? Date { get; set; }

	/// <summary>The custom field values to set.</summary>
	[JsonPropertyName("customFields")]
	[JsonPropertyOrder(510)]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The summary of the triage findings.</summary>
	[JsonPropertyName("summary")]
	[JsonPropertyOrder(520)]
	public string? Summary { get; set; }

	/// <summary>The name of an existing alert status. The server default is <c>New</c>.</summary>
	[JsonPropertyName("status")]
	[JsonPropertyOrder(530)]
	public string? Status { get; set; }

	/// <summary>The login of the user to assign; unassigned when omitted.</summary>
	[JsonPropertyName("assignee")]
	[JsonPropertyOrder(540)]
	public string? Assignee { get; set; }

	/// <summary>The name of a case template to apply when a case is created from the alert.</summary>
	[JsonPropertyName("caseTemplate")]
	[JsonPropertyOrder(550)]
	public string? CaseTemplate { get; set; }

	/// <summary>The observables to attach to the alert.</summary>
	[JsonPropertyName("observables")]
	[JsonPropertyOrder(560)]
	public List<ObservableInput>? Observables { get; set; }

	/// <summary>The tactics, techniques and procedures (TTPs) to link to the alert.</summary>
	[JsonPropertyName("procedures")]
	[JsonPropertyOrder(570)]
	public List<ProcedureInput>? Procedures { get; set; }
}
