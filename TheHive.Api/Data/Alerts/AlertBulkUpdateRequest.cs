using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Alerts;

/// <summary>
/// The body of a bulk update-alert request (the spec's <c>InputUpdateAlertWithIds</c>): the fields of an
/// <see cref="AlertUpdateRequest"/> applied to every alert in <see cref="Ids"/>. Only set properties are sent.
/// </summary>
public sealed class AlertBulkUpdateRequest : AlertUpdateRequest
{
	/// <summary>The alerts to update: IDs preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	[JsonPropertyOrder(-1)]
	public required List<string> Ids { get; set; }
}
