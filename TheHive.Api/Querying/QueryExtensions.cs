using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using TheHive.Api.Data.Query;
using TheHive.Api.Interfaces;

namespace TheHive.Api.Querying;

/// <summary>Runs <see cref="QueryBuilder"/> queries through <see cref="IQuery"/> and maps the results.</summary>
public static class QueryExtensions
{
	/// <summary>Runs a query and deserializes the results with <see cref="TheHiveJson.Options"/>.</summary>
	/// <typeparam name="T">The result type, for example <see cref="Data.Cases.Case"/> for <see cref="QueryBuilder.ListCases"/>.</typeparam>
	/// <param name="query">The query operations.</param>
	/// <param name="builder">The query; it must not end with <see cref="QueryBuilder.Count"/> (use <see cref="RunCountAsync"/>).</param>
	/// <param name="options">A label for the query (<see cref="QueryRunOptions.Name"/>), sent as the <c>name</c> query-string parameter; pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The results; a JSON <c>null</c> gives an empty list and a single object a one-item list (TheHive 5.8.0 answers
	/// <c>getXxx</c> with an array).
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="query"/>, <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="builder"/> ends with a <c>count</c> step.</exception>
	/// <exception cref="InvalidOperationException">
	/// The query returned a single value (number, string or boolean) rather than entities, because its last operation returns a
	/// value directly (for example <c>countFreetags</c>); read it with <see cref="IQuery.RunAsync"/>.
	/// </exception>
	public static async Task<List<T>> RunAsync<T>(this IQuery query, QueryBuilder builder, QueryRunOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);
		ArgumentNullException.ThrowIfNull(options);
		ThrowIfCount(builder);
		var result = await query.RunAsync(builder.Build(), options, cancellationToken).ConfigureAwait(false);
		return ToList<T>(result);
	}

	/// <summary>Runs a query and returns its results with the total number of matching results (the <c>X-Total</c> header).</summary>
	/// <typeparam name="T">The result type.</typeparam>
	/// <param name="query">The query operations.</param>
	/// <param name="builder">The query, usually ending with <see cref="QueryBuilder.Page"/> whose <c>extraData</c> includes <c>total</c>.</param>
	/// <param name="options">A label for the query (<see cref="QueryRunOptions.Name"/>), sent as the <c>name</c> query-string parameter; pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The results, and the total when the server sent it.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="query"/>, <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="builder"/> ends with a <c>count</c> step.</exception>
	/// <exception cref="TheHiveApiException">The server returned an error status.</exception>
	/// <exception cref="InvalidOperationException">The query returned a single value (number, string or boolean) rather than entities.</exception>
	public static async Task<QueryPage<T>> RunPageAsync<T>(this IQuery query, QueryBuilder builder, QueryRunOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);
		ArgumentNullException.ThrowIfNull(options);
		ThrowIfCount(builder);
		using var response = await query.RunUncheckedAsync(builder.Build(), options, cancellationToken).ConfigureAwait(false);
		var error = await TheHiveErrorMapper.CreateAsync(response).ConfigureAwait(false);
		if (error is not null)
		{
			throw error;
		}

		var total = response.Headers.TryGetValues("X-Total", out var values)
			&& long.TryParse(values.First(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
				? parsed
				: (long?)null;
		var result = await response.Content.ReadFromJsonAsync<JsonElement>(TheHiveJson.Options, cancellationToken).ConfigureAwait(false);
		return new QueryPage<T> { Items = ToList<T>(result), Total = total };
	}

	/// <summary>Runs a query that ends with <see cref="QueryBuilder.Count"/> and returns the number.</summary>
	/// <param name="query">The query operations.</param>
	/// <param name="builder">The query; it must end with <see cref="QueryBuilder.Count"/>.</param>
	/// <param name="options">A label for the query (<see cref="QueryRunOptions.Name"/>), sent as the <c>name</c> query-string parameter; pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The number of results.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="query"/>, <paramref name="builder"/> or <paramref name="options"/> is <see langword="null"/>.</exception>
	/// <exception cref="ArgumentException"><paramref name="builder"/> does not end with a <c>count</c> step.</exception>
	/// <exception cref="InvalidOperationException">The server returned something other than a number.</exception>
	public static async Task<long> RunCountAsync(this IQuery query, QueryBuilder builder, QueryRunOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(options);
		if (!builder.EndsWithCount)
		{
			throw new ArgumentException("End the query with Count() to count its results.", nameof(builder));
		}

		var result = await query.RunAsync(builder.Build(), options, cancellationToken).ConfigureAwait(false);
		return result.ValueKind == JsonValueKind.Number
			? result.GetInt64()
			: throw new InvalidOperationException($"The count query returned {result.ValueKind}, not a number.");
	}

	/// <summary>
	/// Runs a query and downloads the results as a file (an unstable route). The query is sent as the JSON text of its
	/// operations array (the request's <c>query</c>); <c>includeFields</c> and <c>excludeFields</c> do not apply to exports.
	/// </summary>
	/// <param name="query">The query operations.</param>
	/// <param name="builder">The query; it must return entities of <see cref="ExportOptions.Model"/>.</param>
	/// <param name="options">The export options.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The file, as for <see cref="IQuery.ExportAsync"/>; dispose it.</returns>
	public static Task<HttpContent> ExportAsync(this IQuery query, QueryBuilder builder, ExportOptions options, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(query);
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(options);
		return query.ExportAsync(
			JsonSerializer.Serialize(builder.Build().Query, TheHiveJson.Options),
			JsonSerializer.Serialize(options, TheHiveJson.Options),
			cancellationToken);
	}

	private static void ThrowIfCount(QueryBuilder builder)
	{
		ArgumentNullException.ThrowIfNull(builder);
		if (builder.EndsWithCount)
		{
			throw new ArgumentException("A query ending with Count() returns a number; use RunCountAsync.", nameof(builder));
		}
	}

	private static List<T> ToList<T>(JsonElement result) => result.ValueKind switch
	{
		JsonValueKind.Array => result.Deserialize<List<T>>(TheHiveJson.Options)!,
		JsonValueKind.Object => [result.Deserialize<T>(TheHiveJson.Options)!],
		JsonValueKind.Null => [],
		_ => throw new InvalidOperationException(
			$"The query returned a single value ({result.ValueKind}), not entities: its last operation (for example countFreetags or "
			+ "similarCaseLightCount) returns a value directly. Use IQuery.RunAsync to read the raw JSON, or RunCountAsync for a count.")
	};
}
