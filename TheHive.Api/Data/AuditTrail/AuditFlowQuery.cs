using Refit;

namespace TheHive.Api.Data.AuditTrail;

/// <summary>
/// The optional query-string parameters of <c>IAudit.GetFlowAsync</c>. Each property is sent as the query parameter of the same wire name and is
/// left out when <see langword="null"/>; pass an empty instance (<c>new()</c>) for the most recent entries across all visible object types.
/// </summary>
public sealed class AuditFlowQuery
{
	/// <summary>
	/// The case ID preceded by <c>~</c> (the case number is not accepted) to scope the trail to one case, sent as <c>rootId</c>; omit it, or pass
	/// <c>any</c>, for the most recent entries across all visible object types.
	/// </summary>
	[AliasAs("rootId")]
	public string? RootId { get; set; }

	/// <summary>The maximum number of entries to return, sent as <c>count</c>; the server default is 10, used when this is <see langword="null"/>.</summary>
	[AliasAs("count")]
	public int? Count { get; set; }
}