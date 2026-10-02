using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>
/// The body of a bulk update-case request (the spec's <c>InputUpdateCaseWithIds</c>): the fields of a
/// <see cref="CaseUpdateRequest"/> applied to every case in <see cref="Ids"/>. Only set properties are sent.
/// </summary>
public sealed class CaseBulkUpdateRequest : CaseUpdateRequest
{
	/// <summary>The cases to update: IDs preceded by <c>~</c>, or case numbers.</summary>
	[JsonPropertyName("ids")]
	[JsonPropertyOrder(-1)]
	public required List<string> Ids { get; set; }
}
