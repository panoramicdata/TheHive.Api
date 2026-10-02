using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>An additional parameter included in the OAuth 2.0 token request (the spec's <c>TokenParameter</c>).</summary>
public sealed class OAuthTokenParameter
{
	/// <summary>The parameter key.</summary>
	[JsonPropertyName("key")]
	public string Key { get; set; } = string.Empty;

	/// <summary>The parameter value. SECRET when it carries credentials: it only ever travels in a request or response body.</summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;
}
