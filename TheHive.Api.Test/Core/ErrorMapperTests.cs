using System.Net;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public class ErrorMapperTests
{
	private static HttpResponseMessage Response(HttpStatusCode status, string body, Action<HttpResponseMessage>? configure = null)
	{
		var response = new HttpResponseMessage(status) { Content = new StringContent(body) };
		configure?.Invoke(response);
		return response;
	}

	[Fact]
	public async Task Create_SuccessResponse_ReturnsNull()
	{
		using var response = Response(HttpStatusCode.OK, "{}");
		(await TheHiveErrorMapper.CreateAsync(response)).Should().BeNull();
	}

	[Fact]
	public async Task Create_JsonError_PopulatesFields()
	{
		using var response = Response(HttpStatusCode.NotFound, """{"type":"NotFound","message":"nope"}""");
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.StatusCode.Should().Be(HttpStatusCode.NotFound);
		exception.ErrorType.Should().Be("NotFound");
		exception.Message.Should().Be("nope");
		exception.RequestId.Should().BeNull();
	}

	[Fact]
	public async Task Create_RequestIdHeader_IsCaptured()
	{
		using var response = Response(HttpStatusCode.BadRequest, "{}", r => r.Headers.Add("X-Request-Id", "req-1"));
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.RequestId.Should().Be("req-1");
	}

	[Fact]
	public async Task Create_HtmlBody_FallsBackToStatusMessage()
	{
		using var response = Response(HttpStatusCode.BadGateway, "<html>bad gateway</html>");
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.StatusCode.Should().Be(HttpStatusCode.BadGateway);
		exception.Message.Should().Be("HTTP 502 (Bad Gateway)");
		exception.ErrorType.Should().BeNull();
	}

	[Fact]
	public async Task Create_EmptyBody_FallsBackToStatusMessage()
	{
		using var response = Response(HttpStatusCode.InternalServerError, string.Empty);
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.Message.Should().Be("HTTP 500 (Internal Server Error)");
	}

	[Fact]
	public async Task Create_NoContentAssigned_FallsBackToStatusMessage()
	{
		using var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.Message.Should().Be("HTTP 400 (Bad Request)");
	}

	[Fact]
	public async Task Create_UnknownStatusWithoutReasonPhrase_UsesStatusName()
	{
		using var response = Response((HttpStatusCode)599, string.Empty);
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.Message.Should().Be("HTTP 599 (599)");
	}

	[Fact]
	public async Task Create_JsonWithoutMessage_FallsBack()
	{
		using var response = Response(HttpStatusCode.BadRequest, """{"type":"InvalidFormat"}""");
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.ErrorType.Should().Be("InvalidFormat");
		exception.Message.Should().Be("HTTP 400 (Bad Request)");
	}

	[Fact]
	public async Task Create_JsonWithoutType_HasNullType()
	{
		using var response = Response(HttpStatusCode.BadRequest, """{"message":"bad"}""");
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.ErrorType.Should().BeNull();
		exception.Message.Should().Be("bad");
	}

	[Fact]
	public async Task Create_NonObjectJson_FallsBack()
	{
		using var response = Response(HttpStatusCode.BadRequest, "[1,2]");
		var exception = (await TheHiveErrorMapper.CreateAsync(response)).Should().BeOfType<TheHiveApiException>().Subject;
		exception.Message.Should().Be("HTTP 400 (Bad Request)");
	}

	[Fact]
	public async Task ClientCall_ErrorResponse_MapsToApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NotFound, """{"type":"NotFound","message":"nope"}""");
		using var client = TestClient.Create(stub);
		using var response = await client.HttpClient.GetAsync("api/v1/case/1", TestContext.Current.CancellationToken);
		var exception = await TheHiveErrorMapper.CreateAsync(response);
		exception.Should().BeOfType<TheHiveApiException>().Which.Message.Should().Be("nope");
	}
}
