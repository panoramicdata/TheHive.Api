using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Alerts;

/// <summary>The body of a bulk delete-alerts request.</summary>
public sealed class AlertBulkDeleteRequest
{
	/// <summary>The alerts to delete permanently: IDs preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	public required List<string> Ids { get; set; }
}
