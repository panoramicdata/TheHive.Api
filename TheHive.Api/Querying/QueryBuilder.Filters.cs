namespace TheHive.Api.Querying;

/// <content>Shortcuts that add a one-condition <c>filter</c> step, and grouped filter steps.</content>
public sealed partial class QueryBuilder
{
	/// <summary>Adds a <c>filter</c> step: field equals the value (<c>_eq</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value; <see langword="null"/> is sent as JSON <c>null</c>.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Filter(string field, object? value) => Filter(f => f.Eq(field, value));

	/// <summary>Adds a <c>filter</c> step: field equals one of the values (<c>_in</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="values">The values; at least one.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder FilterIn(string field, params object?[] values) => Filter(f => f.In(field, values));

	/// <summary>Adds a <c>filter</c> step: field contains the text (<c>_like</c>; <c>*</c> is a wildcard).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The text.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder FilterLike(string field, string value) => Filter(f => f.Like(field, value));

	/// <summary>Adds a <c>filter</c> step: field is greater than the value (<c>_gt</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder FilterGt(string field, object? value) => Filter(f => f.Gt(field, value));

	/// <summary>Adds a <c>filter</c> step: field is less than the value (<c>_lt</c>).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="value">The value.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder FilterLt(string field, object? value) => Filter(f => f.Lt(field, value));

	/// <summary>Adds a <c>filter</c> step: field is in a range (<c>_between</c>; <paramref name="from"/> inclusive, <paramref name="to"/> exclusive).</summary>
	/// <param name="field">The field name.</param>
	/// <param name="from">The inclusive lower bound.</param>
	/// <param name="to">The exclusive upper bound.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder FilterBetween(string field, object? from, object? to) => Filter(f => f.Between(field, from, to));

	/// <summary>Adds a <c>filter</c> step: all the conditions added by <paramref name="conditions"/> match (<c>_and</c>).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder And(Action<FilterBuilder> conditions) => Filter(f => f.And(conditions));

	/// <summary>Adds a <c>filter</c> step: at least one of the conditions added by <paramref name="conditions"/> matches (<c>_or</c>).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Or(Action<FilterBuilder> conditions) => Filter(f => f.Or(conditions));

	/// <summary>Adds a <c>filter</c> step: the conditions added by <paramref name="conditions"/> do not all match (<c>_not</c>).</summary>
	/// <param name="conditions">Adds at least one condition.</param>
	/// <returns>This builder.</returns>
	public QueryBuilder Not(Action<FilterBuilder> conditions) => Filter(f => f.Not(conditions));
}
