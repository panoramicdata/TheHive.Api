using System.Net;
using System.Net.Http.Headers;

namespace TheHive.Api.Test.Support;

/// <summary>One part of a recorded multipart/form-data body.</summary>
internal sealed record RecordedPart(string? Name, string? FileName, string? ContentType, byte[] Bytes)
{
	public string Text => System.Text.Encoding.UTF8.GetString(Bytes);
}

internal sealed record RecordedCall(HttpMethod Method, Uri Uri, string? Body, HttpRequestHeaders Headers)
{
	/// <summary>The request Content-Type media type, or <see langword="null"/> without a body.</summary>
	public string? ContentType { get; init; }

	/// <summary>The raw bytes of a non-multipart body, or <see langword="null"/>.</summary>
	public byte[]? BodyBytes { get; init; }

	/// <summary>The parts of a multipart body, in order; empty for other bodies.</summary>
	public IReadOnlyList<RecordedPart> Parts { get; init; } = [];
}

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

	/// <summary>Queues a binary download: the given bytes, media type and <c>attachment; filename=...</c> disposition.</summary>
	public void EnqueueFile(byte[] bytes, string contentType, string fileName)
		=> _responses.Enqueue(() =>
		{
			var content = new ByteArrayContent(bytes);
			content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") { FileName = fileName };
			return new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
		});

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Each body is read exactly once, as a real transport would, so non-seekable streams work.
		// A multipart body is recorded part by part (Body and BodyBytes stay null); any other body as bytes and UTF-8 text.
		var content = request.Content;
		var parts = new List<RecordedPart>();
		byte[]? bytes = null;
		if (content is MultipartContent multipart)
		{
			foreach (var part in multipart)
			{
				var disposition = part.Headers.ContentDisposition;
				parts.Add(new RecordedPart(
					disposition?.Name?.Trim('"'),
					disposition?.FileName?.Trim('"'),
					part.Headers.ContentType?.MediaType,
					await part.ReadAsByteArrayAsync(cancellationToken)));
			}
		}
		else if (content is not null)
		{
			bytes = await content.ReadAsByteArrayAsync(cancellationToken);
		}

		var body = bytes is null ? null : System.Text.Encoding.UTF8.GetString(bytes);
		Calls.Add(new RecordedCall(request.Method, request.RequestUri!, body, request.Headers)
		{
			ContentType = content?.Headers.ContentType?.MediaType,
			BodyBytes = bytes,
			Parts = parts
		});
		return _responses.Dequeue()();
	}
}
