using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Alerts;

/// <summary>The body of a merge-several-alerts-into-a-case request (the spec's <c>InputAlertsMergeWithCase</c>).</summary>
public sealed class AlertBulkMergeRequest
{
	/// <summary>The case to merge into: its ID preceded by <c>~</c>, or its case number.</summary>
	[JsonPropertyName("caseId")]
	public required string CaseId { get; set; }

	/// <summary>The alerts to merge: IDs preceded by <c>~</c>. The server allows 50 by default.</summary>
	[JsonPropertyName("alertIds")]
	public List<string>? AlertIds { get; set; }
}
