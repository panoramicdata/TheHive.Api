using System.Net;
using System.Net.Http.Headers;

namespace TheHive.Api.Test.Support;

internal sealed record RecordedCall(HttpMethod Method, Uri Uri, string? Body, HttpRequestHeaders Headers);

internal sealed class StubHandler : HttpMessageHandler
{
	private readonly Queue<Func<HttpResponseMessage>> _responses = new();

	public List<RecordedCall> Calls { get; } = [];

	public void Enqueue(HttpStatusCode status, string json = "{}", Action<HttpResponseMessage>? configure = null)
		=> _responses.Enqueue(() =>
		{
			var response = new HttpResponseMessage(status)
			{
				Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
			};
			configure?.Invoke(response);
			return response;
		});

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
		Calls.Add(new RecordedCall(request.Method, request.RequestUri!, body, request.Headers));
		return _responses.Dequeue()();
	}
}
