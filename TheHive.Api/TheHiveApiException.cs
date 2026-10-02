using System.Net;

namespace TheHive.Api;

/// <summary>Raised when TheHive returns a non-success response.</summary>
public sealed class TheHiveApiException(HttpStatusCode statusCode, string? errorType, string message, string? requestId)
	: Exception(message)
{
	/// <summary>The HTTP status code.</summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <summary>TheHive's error <c>type</c>, when supplied.</summary>
	public string? ErrorType { get; } = errorType;

	/// <summary>The <c>X-Request-Id</c> response header, when supplied.</summary>
	public string? RequestId { get; } = requestId;
}
