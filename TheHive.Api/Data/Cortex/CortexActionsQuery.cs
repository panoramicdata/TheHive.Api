using Refit;

namespace TheHive.Api.Data.Cortex;

/// <summary>
/// The optional query-string parameters of <c>ICortex.ListActionsAsync</c>: a filter, sort criteria and a page. Each property is sent as the
/// query parameter of the same wire name and is left out when <see langword="null"/>.
/// </summary>
public sealed class CortexActionsQuery
{
	/// <summary>
	/// A filter as JSON text, in the syntax of the <c>filter</c> operation of the query API without its <c>_name</c>, for example
	/// <c>{"_eq":{"_field":"status","_value":"Success"}}</c>, sent as <c>filter</c>. The spec declares this parameter as JSON-encoded content; it is
	/// URL-encoded as one query value.
	/// </summary>
	[AliasAs("filter")]
	public string? Filter { get; set; }

	/// <summary>
	/// Sort criteria as JSON text, in the syntax of the <c>_fields</c> value of the query API's <c>sort</c> operation, for example
	/// <c>[{"field":"responderId","direction":"asc"}]</c>, sent as <c>sort</c>.
	/// </summary>
	[AliasAs("sort")]
	public string? Sort { get; set; }

	/// <summary>The 0-based index of the first result (server default 0), sent as <c>pageFrom</c>.</summary>
	[AliasAs("pageFrom")]
	public int? PageFrom { get; set; }

	/// <summary>The exclusive index of the last result (server default <see cref="PageFrom"/> + 30; the range cannot exceed 300), sent as <c>pageTo</c>.</summary>
	[AliasAs("pageTo")]
	public int? PageTo { get; set; }
}
