using Refit;

namespace TheHive.Api.Data.Query;

/// <summary>
/// The optional query-string parameter of <c>IQuery.RunAsync</c>, <c>IQuery.RunUncheckedAsync</c> and the typed helpers in
/// <c>Querying.QueryExtensions</c>. Pass an empty instance (<c>new()</c>) to leave it out.
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