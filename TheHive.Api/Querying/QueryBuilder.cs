using System.Text.Json.Nodes;
using TheHive.Api.Data.Query;

namespace TheHive.Api.Querying;

/// <summary>
/// Builds a <see cref="QueryRequest"/> for <c>POST /api/v1/query</c> fluently. A query starts with a primary operation
/// (<see cref="List"/>, <see cref="Get"/> or a typed starter such as <see cref="ListCases"/>), may chain related object
/// operations (<see cref="Related"/>), filter and sort steps, and may end with <see cref="Page"/> or <see cref="Count"/>.
/// </summary>
/// <example>
/// <code>
/// var query = QueryBuilder.ListCases()
///     .FilterLike("title", "ransomware")
///     .Sort("_createdAt", SortDirection.Descending)
///     .Page(0, 15, "total");
/// var cases = await client.Query.RunAsync&lt;Case&gt;(query);
/// </code>
/// </example>
public sealed partial class QueryBuilder
{
	private static readonly HashSet<string> NonPrimaryOperations = ["filter", "sort", "page", "count", "output"];

	private readonly List<JsonObject> _steps = [];
	private readonly List<string> _includeFields = [];
	private readonly List<string> _excludeFields = [];
	private JsonArray? _openSortFields;
	private string? _lastStep;

	private QueryBuilder(JsonObject primary)
	{
		_steps.Add(primary);
	}

	/// <summary>Whether the query ends with a <c>count</c> step, so it returns a number rather than entities.</summary>
	public bool EndsWithCount => _lastStep == "count";

	/// <summary>Starts a query with a primary operation that takes no parameter, such as <c>listCase</c>, <c>myTasks</c> or <c>currentUser</c>.</summary>
	/// <param name="operationName">The operation name, as listed in the spec's "Primary operations by entity".</param>
	/// <returns>A new builder.</returns>
	/// <exception cref="ArgumentException"><paramref name="operationName"/> is blank or not a primary operation.</exception>
	public static QueryBuilder List(string operationName) => new(Primary(operationName));

	/// <summary>Starts a query with a primary operation that gets one entity, such as <c>getCase</c>.</summary>
	/// <param name="operationName">The operation name, as listed in the spec's "Primary operations by entity".</param>
	/// <param name="idOrName">The entity ID preceded by <c>~</c>, or the entity's own identifier (case number, login, name...).</param>
	/// <returns>A new builder.</returns>
	/// <exception cref="ArgumentException"><paramref name="operationName"/> or <paramref name="idOrName"/> is blank, or the operation is not a primary operation.</exception>
	public static QueryBuilder Get(string operationName, string idOrName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(idOrName);
		var step = Primary(operationName);
		step["idOrName"] = idOrName;
		return new(step);
	}

	/// <summary>Chains a related object operation, such as <c>tasks</c>, <c>observables</c> or <c>logs</c>.</summary>
	/// <param name="operationName">The operation name, as listed in the spec's "Related object operations by entity".</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Related(string operationName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
		return AddStep(new JsonObject { ["_name"] = operationName });
	}

	/// <summary>Adds a <c>filter</c> step with the conditions added by <paramref name="conditions"/> (all must match).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	/// <exception cref="ArgumentException">No condition was added.</exception>
	public QueryBuilder Filter(Action<FilterBuilder> conditions)
	{
		var (op, operand) = FilterBuilder.Nested(conditions).ToCondition();
		return AddStep(new JsonObject { ["_name"] = "filter", [op] = operand });
	}

	/// <summary>Sorts by a field. Consecutive calls add fields to the same <c>sort</c> step, in priority order.</summary>
	/// <param name="field">The field name.</param>
	/// <param name="direction">The direction; ascending by default.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Sort(string field, SortDirection direction = SortDirection.Ascending)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(field);
		var wire = direction switch
		{
			SortDirection.Ascending => "asc",
			SortDirection.Descending => "desc",
			_ => throw new ArgumentOutOfRangeException(nameof(direction), direction, "Unknown sort direction.")
		};
		if (_openSortFields is null)
		{
			var fields = new JsonArray();
			AddStep(new JsonObject { ["_name"] = "sort", ["_fields"] = fields });
			_openSortFields = fields;
		}

		_openSortFields.Add(new JsonObject { [field] = wire });
		return this;
	}

	/// <summary>Ends the query with a <c>page</c> step: results <paramref name="from"/> (inclusive) to <paramref name="to"/> (exclusive).</summary>
	/// <param name="from">The 0-based index of the first result.</param>
	/// <param name="to">The exclusive index of the last result; greater than <paramref name="from"/>.</param>
	/// <param name="extraData">
	/// Computed fields to add to each result's <c>extraData</c>, such as <c>taskStats</c>; <c>total</c> returns the number of
	/// matching results in the <c>X-Total</c> header (read by <see cref="QueryExtensions.RunPageAsync"/>).
	/// </param>
	/// <returns>This builder.</returns>
	public QueryBuilder Page(int from, int to, params string[] extraData)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(from);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(to, from);
		ThrowIfAnyBlank(extraData, nameof(extraData));
		var step = new JsonObject { ["_name"] = "page", ["from"] = from, ["to"] = to };
		if (extraData.Length > 0)
		{
			step["extraData"] = new JsonArray([.. extraData.Select(e => (JsonNode)e)]);
		}

		return AddStep(step);
	}

	/// <summary>Ends the query with a <c>count</c> step: the server returns the number of results instead of the results.</summary>
	/// <returns>This builder.</returns>
	public QueryBuilder Count() => AddStep(new JsonObject { ["_name"] = "count" });

	/// <summary>Keeps only these fields in every result (<c>includeFields</c>); all other fields are omitted.</summary>
	/// <param name="fields">The field names; at least one.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Select(params string[] fields)
	{
		ThrowIfEmptyOrAnyBlank(fields);
		_includeFields.AddRange(fields);
		return this;
	}

	/// <summary>Omits these fields from every result (<c>excludeFields</c>); ignored by the server when <see cref="Select"/> is used.</summary>
	/// <param name="fields">The field names; at least one.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Exclude(params string[] fields)
	{
		ThrowIfEmptyOrAnyBlank(fields);
		_excludeFields.AddRange(fields);
		return this;
	}

	/// <summary>Creates the request. Each call returns an independent copy, so the builder can be extended and built again.</summary>
	/// <returns>The request body.</returns>
	public QueryRequest Build() => new()
	{
		Query = [.. _steps.Select(s => (JsonObject)s.DeepClone())],
		IncludeFields = _includeFields.Count == 0 ? null : [.. _includeFields],
		ExcludeFields = _excludeFields.Count == 0 ? null : [.. _excludeFields]
	};

	private static JsonObject Primary(string operationName)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(operationName);
		if (NonPrimaryOperations.Contains(operationName))
		{
			throw new ArgumentException($"A query must start with a primary operation (listXxx or getXxx), not '{operationName}'.", nameof(operationName));
		}

		return new JsonObject { ["_name"] = operationName };
	}

	private static void ThrowIfEmptyOrAnyBlank(string[] fields)
	{
		ArgumentNullException.ThrowIfNull(fields);
		if (fields.Length == 0)
		{
			throw new ArgumentException("Pass at least one field.", nameof(fields));
		}

		ThrowIfAnyBlank(fields, nameof(fields));
	}

	private static void ThrowIfAnyBlank(string[] values, string parameterName)
	{
		ArgumentNullException.ThrowIfNull(values, parameterName);
		if (values.Any(string.IsNullOrWhiteSpace))
		{
			throw new ArgumentException("Names must not be blank.", parameterName);
		}
	}

	private QueryBuilder AddStep(JsonObject step)
	{
		if (_lastStep is "page" or "count")
		{
			throw new InvalidOperationException($"The '{_lastStep}' step must be the last step of a query.");
		}

		_steps.Add(step);
		_lastStep = (string)step["_name"]!;
		_openSortFields = null;
		return this;
	}
}
