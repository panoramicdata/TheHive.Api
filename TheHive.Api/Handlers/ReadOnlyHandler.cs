namespace TheHive.Api.Handlers;

/// <summary>
/// Refuses every request other than GET, HEAD, OPTIONS or the read-only query POST while <see cref="TheHiveClientOptions.IsReadOnly"/> is set.
/// The option is read on every request.
/// </summary>
internal sealed class ReadOnlyHandler(TheHiveClientOptions options) : DelegatingHandler
{
	// POST api/v1/query only reads: it is how TheHive lists and searches.
	private const string QueryPath = "/api/v1/query";

	protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		if (options.IsReadOnly && !IsRead(request))
		{
			throw new InvalidOperationException($"The client is read-only (IsReadOnly is true): {request.Method} {request.RequestUri?.AbsolutePath} is not allowed.");
		}

		return base.SendAsync(request, cancellationToken);
	}

	private static bool IsRead(HttpRequestMessage request)
		=> request.Method == HttpMethod.Get
			|| request.Method == HttpMethod.Head
			|| request.Method == HttpMethod.Options
			|| (request.Method == HttpMethod.Post
				&& request.RequestUri is { } uri
				&& uri.AbsolutePath.EndsWith(QueryPath, StringComparison.OrdinalIgnoreCase));
}
