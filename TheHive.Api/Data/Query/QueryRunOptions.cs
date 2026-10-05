using Refit;

namespace TheHive.Api.Data.Query;

/// <summary>
/// The optional query-string parameter of <see cref="Interfaces.IQuery.RunAsync"/>, <see cref="Interfaces.IQuery.RunUncheckedAsync"/> and the typed helpers in
/// <see cref="Querying.QueryExtensions"/>. Pass an empty instance (<c>new()</c>) to leave it out.
/// </summary>
public sealed class QueryRunOptions
{
	/// <summary>
	/// A label for the query, sent as the <c>name</c> query-string parameter; left out when <see langword="null"/>. Not in the spec; TheHive 5.8.0
	/// accepts it (checked against a live server).
	/// </summary>
	[AliasAs("name")]
	public string? Name { get; set; }
}