using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Procedures;

/// <summary>The body of a bulk delete-procedures request.</summary>
public sealed class ProcedureBulkDeleteRequest
{
	/// <summary>The procedures to delete permanently: IDs preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	public required List<string> Ids { get; set; }
}
