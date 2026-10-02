using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Tasks;

/// <summary>
/// The body of a bulk update-task request (the spec's <c>InputUpdateTaskWithIds</c>): the fields of a
/// <see cref="CaseTaskUpdateRequest"/> applied to every task in <see cref="Ids"/>. Only set properties are sent.
/// </summary>
public sealed class CaseTaskBulkUpdateRequest : CaseTaskUpdateRequest
{
	/// <summary>The tasks to update: IDs preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	[JsonPropertyOrder(-1)]
	public required List<string> Ids { get; set; }
}
