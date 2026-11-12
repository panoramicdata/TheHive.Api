using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cortex;

/// <summary>An analyzer job on an observable (the spec's <c>OutputJob</c>).</summary>
public sealed class CortexJob : TheHiveEntity
{
	/// <summary>The ID of the analyzer that ran.</summary>
	[JsonPropertyName("analyzerId")]
	public string AnalyzerId { get; set; } = string.Empty;

	/// <summary>The display name of the analyzer that ran.</summary>
	[JsonPropertyName("analyzerName")]
	public string AnalyzerName { get; set; } = string.Empty;

	/// <summary>
	/// The definition of the analyzer that ran, as returned by Cortex. The spec declares a string but its example is an object,
	/// so this is kept as raw JSON and reads either form.
	/// </summary>
	[JsonPropertyName("analyzerDefinition")]
	public JsonElement? AnalyzerDefinition { get; set; }

	/// <summary>The status of the job: <c>Waiting</c>, <c>InProgress</c>, <c>Success</c>, <c>Failure</c> or <c>Deleted</c> (the spec does not enumerate it).</summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;

	/// <summary>When the job started.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset StartDate { get; set; }

	/// <summary>When the job finished; absent while it is still running.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>The Cortex analysis report, once the analyzer has completed; free-form JSON (the spec only says object).</summary>
	[JsonPropertyName("report")]
	public JsonElement? Report { get; set; }

	/// <summary>The name of the Cortex server the analyzer ran on.</summary>
	[JsonPropertyName("cortexId")]
	public string CortexId { get; set; } = string.Empty;

	/// <summary>The ID of the corresponding job on the Cortex server.</summary>
	[JsonPropertyName("cortexJobId")]
	public string CortexJobId { get; set; } = string.Empty;

	/// <summary>The internal identifier of the job, identical to <see cref="TheHive.Api.Data.Common.TheHiveAuditedEntity.Id"/>.</summary>
	[JsonPropertyName("id")]
	public string JobId { get; set; } = string.Empty;

	/// <summary>The observable the job analyzed; free-form JSON (the spec only says object).</summary>
	[JsonPropertyName("case_artifact")]
	public JsonElement? CaseArtifact { get; set; }

	/// <summary>
	/// The follow-up operations produced by the analyzer report. The spec declares a serialized JSON string but its example is an
	/// array, so this is kept as raw JSON and reads either form.
	/// </summary>
	[JsonPropertyName("operations")]
	public JsonElement? Operations { get; set; }
}
