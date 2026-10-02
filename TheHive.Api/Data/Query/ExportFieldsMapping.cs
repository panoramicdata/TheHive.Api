using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Query;

/// <summary>The fields available for CSV export, by model (the spec's <c>OutputExportFieldsMapping</c>).</summary>
public sealed class ExportFieldsMapping
{
	/// <summary>The exportable fields keyed by model name (<c>Case</c>, <c>Alert</c>, <c>User</c>, <c>Organisation</c>, <c>Procedure</c>, <c>Task</c>, <c>Observable</c>).</summary>
	[JsonPropertyName("fieldsByModel")]
	public Dictionary<string, List<ExportFieldDescription>> FieldsByModel { get; set; } = [];
}
