using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Query;

/// <summary>
/// The body of <c>POST /api/v1/query</c> (the spec's <c>InputQuery</c>): a chain of operations run in order. Build it with
/// <see cref="Querying.QueryBuilder"/>, or write the steps by hand for operations the builder does not cover.
/// </summary>
public sealed class QueryRequest
{
	/// <summary>
	/// The operations, in order. Each is a JSON object whose <c>_name</c> identifies the operation (for example
	/// <c>{"_name":"listCase"}</c>, <c>{"_name":"getCase","idOrName":"~1234"}</c>,
	/// <c>{"_name":"filter","_eq":{"_field":"status","_value":"New"}}</c>), followed by its parameters. The first is a primary
	/// operation (<c>listXxx</c> or <c>getXxx</c>).
	/// </summary>
	[JsonPropertyName("query")]
	public required List<JsonObject> Query { get; init; }

	/// <summary>Field names to keep in every result object; all other fields are omitted. Takes precedence over <see cref="ExcludeFields"/>.</summary>
	[JsonPropertyName("includeFields")]
	public List<string>? IncludeFields { get; set; }

	/// <summary>Field names to omit from every result object; ignored when <see cref="IncludeFields"/> is set.</summary>
	[JsonPropertyName("excludeFields")]
	public List<string>? ExcludeFields { get; set; }
}
