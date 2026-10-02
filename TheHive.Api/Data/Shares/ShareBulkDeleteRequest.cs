using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Shares;

/// <summary>The body of the delete-shares request (the spec's inline body of <c>DELETE /api/v1/case/shares</c>).</summary>
public sealed class ShareBulkDeleteRequest
{
	/// <summary>The IDs of the shares to remove, each preceded by <c>~</c>.</summary>
	[JsonPropertyName("ids")]
	public required List<string> Ids { get; set; }
}
