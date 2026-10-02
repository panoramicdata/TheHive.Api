using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Observables;

/// <summary>
/// The body of a bulk update-observable request (the spec's <c>InputUpdateObservableWithIds</c>): the fields of an
/// <see cref="ObservableUpdateRequest"/> applied to every observable in <see cref="Ids"/>. Only set properties are sent.
/// </summary>
public sealed class ObservableBulkUpdateRequest : ObservableUpdateRequest
{
	/// <summary>The observables to update: IDs preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	[JsonPropertyOrder(-1)]
	public required List<string> Ids { get; set; }
}
