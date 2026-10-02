using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The body of a Google Workspace authorization-code exchange. The spec declares an untyped body; the description gives <c>{ "authorizationCode": "&lt;code&gt;" }</c>.</summary>
public sealed class EmailIntakeAuthorizationCodeRequest
{
	/// <summary>SECRET. The OAuth 2.0 authorization code from the Google consent flow; it only ever travels in this request body.</summary>
	[JsonPropertyName("authorizationCode")]
	public required string AuthorizationCode { get; set; }
}
