namespace TheHive.Api.Data.Query;

/// <summary>One page of query results with the total number of matching results.</summary>
/// <typeparam name="T">The result type.</typeparam>
public sealed class QueryPage<T>
{
	/// <summary>The results of this page.</summary>
	public List<T> Items { get; init; } = [];

	/// <summary>
	/// The total number of matching results across all pages (the <c>X-Total</c> header), or <see langword="null"/> when the
	/// server did not send it (the <c>page</c> step's <c>extraData</c> must include <c>total</c>).
	/// </summary>
	public long? Total { get; init; }
}
