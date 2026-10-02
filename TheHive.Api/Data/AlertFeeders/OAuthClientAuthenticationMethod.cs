using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>How the client authenticates to the OAuth 2.0 token endpoint (the spec's <c>ClientAuthenticationMethod</c>).</summary>
public enum OAuthClientAuthenticationMethod
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The client ID and secret are sent in the request body.</summary>
	[JsonStringEnumMemberName("client_secret_post")]
	ClientSecretPost,

	/// <summary>The client ID and secret are sent in an HTTP Basic header.</summary>
	[JsonStringEnumMemberName("client_secret_basic")]
	ClientSecretBasic
}
