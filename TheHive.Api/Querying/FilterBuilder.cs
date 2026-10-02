using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheHive.Api.Querying;

/// <summary>
/// Collects the conditions of one query <c>filter</c> step (see <see cref="QueryBuilder.Filter(Action{FilterBuilder})"/>).
/// Conditions added side by side must all match; group them with <see cref="Or"/>, <see cref="And"/> and <see cref="Not"/>.
/// </summary>
/// <remarks>
/// Values are written with <see cref="TheHiveJson.Options"/>: a <see cref="DateTimeOffset"/> becomes epoch milliseconds, and a
/// relative date can be passed as an object, for example <c>new { amount = 1, unit = "days", look = "behind" }</c> (see the
/// spec's "Date filter values"). Field names are those reported by <see cref="Interfaces.IDescribe"/>.
/// </remarks>
public sealed class FilterBuilder
{
	private readonly List<(string Operator, JsonNode? Operand)> _conditions = [];

	internal FilterBuilder()
	{
	}

	/// <summary>Field equals the value (<c>_eq</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value; <see langword="null"/> is sent as JSON <c>null</c>.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Eq(string field, object? value) => Compare("_eq", field, value);

	/// <summary>Field does not equal the value (<c>_ne</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Ne(string field, object? value) => Compare("_ne", field, value);

	/// <summary>Field is less than the value (<c>_lt</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Lt(string field, object? value) => Compare("_lt", field, value);

	/// <summary>Field is less than or equal to the value (<c>_lte</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Lte(string field, object? value) => Compare("_lte", field, value);

	/// <summary>Field is greater than the value (<c>_gt</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Gt(string field, object? value) => Compare("_gt", field, value);

	/// <summary>Field is greater than or equal to the value (<c>_gte</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Gte(string field, object? value) => Compare("_gte", field, value);

	/// <summary>Field (or a word of it, depending on the index type) contains the text (<c>_like</c>); <c>*</c> is a wildcard.</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The text.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Like(string field, string value) => Compare("_like", field, value);

	/// <summary>Field contains the word (<c>_match</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The word.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Match(string field, string value) => Compare("_match", field, value);

	/// <summary>Field starts with the text (<c>_startsWith</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The text.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder StartsWith(string field, string value) => Compare("_startsWith", field, value);

	/// <summary>Field ends with the text (<c>_endsWith</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The text.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder EndsWith(string field, string value) => Compare("_endsWith", field, value);

	/// <summary>Field is in a range (<c>_between</c>): <paramref name="from"/> is inclusive, <paramref name="to"/> exclusive.</summary>
	/// <param name="field">The field name.</param>
	/// <param name="from">The inclusive lower bound.</param>
	/// <param name="to">The exclusive upper bound.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Between(string field, object? from, object? to)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(field);
		return Add("_between", new JsonObject { ["_field"] = field, ["_from"] = ToNode(from), ["_to"] = ToNode(to) });
	}

	/// <summary>Field equals one of the values (<c>_in</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="values">The values; at least one.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder In(string field, params object?[] values)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(field);
		ArgumentNullException.ThrowIfNull(values);
		if (values.Length == 0)
		{
			throw new ArgumentException("Pass at least one value.", nameof(values));
		}

		return Add("_in", new JsonObject { ["_field"] = field, ["_values"] = new JsonArray([.. values.Select(ToNode)]) });
	}

	/// <summary>The entity has the field (<c>_has</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Has(string field)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(field);
		return Add("_has", field);
	}

	/// <summary>The entity has the ID (<c>_id</c>).</summary>
	/// <param name="id">The ID preceded by <c>~</c>.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Id(string id)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(id);
		return Add("_id", id);
	}

	/// <summary>Matches any entity (<c>_any</c>).</summary>
	/// <returns>This builder.</returns>
	public FilterBuilder Any() => Add("_any", null);

	/// <summary>All the conditions added by <paramref name="conditions"/> match (<c>_and</c>).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder And(Action<FilterBuilder> conditions) => Add("_and", Nested(conditions).ToArray());

	/// <summary>At least one of the conditions added by <paramref name="conditions"/> matches (<c>_or</c>).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Or(Action<FilterBuilder> conditions) => Add("_or", Nested(conditions).ToArray());

	/// <summary>The conditions added by <paramref name="conditions"/> do not all match (<c>_not</c>).</summary>
	/// <param name="conditions">Adds at least one condition; several are combined with <c>_and</c> before negation.</param>
	/// <returns>This builder.</returns>
	public FilterBuilder Not(Action<FilterBuilder> conditions)
	{
		var (op, operand) = Nested(conditions).ToCondition();
		return Add("_not", new JsonObject { [op] = operand });
	}

	/// <summary>Creates the builder for one filter step, runs <paramref name="conditions"/> on it and checks it is not empty.</summary>
	internal static FilterBuilder Nested(Action<FilterBuilder> conditions)
	{
		ArgumentNullException.ThrowIfNull(conditions);
		var builder = new FilterBuilder();
		conditions(builder);
		if (builder._conditions.Count == 0)
		{
			throw new ArgumentException("A filter needs at least one condition.", nameof(conditions));
		}

		return builder;
	}

	/// <summary>The single condition, or the conditions combined with <c>_and</c>; call once, the nodes are moved.</summary>
	internal (string Operator, JsonNode? Operand) ToCondition()
		=> _conditions.Count == 1 ? _conditions[0] : ("_and", ToArray());

	internal static JsonNode? ToNode(object? value) => JsonSerializer.SerializeToNode(value, TheHiveJson.Options);

	private JsonArray ToArray() => [.. _conditions.Select(c => new JsonObject { [c.Operator] = c.Operand })];

	private FilterBuilder Compare(string op, string field, object? value)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(field);
		return Add(op, new JsonObject { ["_field"] = field, ["_value"] = ToNode(value) });
	}

	private FilterBuilder Add(string op, JsonNode? operand)
	{
		_conditions.Add((op, operand));
		return this;
	}
}
