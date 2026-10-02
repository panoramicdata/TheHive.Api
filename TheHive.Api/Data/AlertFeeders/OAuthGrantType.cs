using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The OAuth 2.0 grant type of an alert feeder (the spec's <c>GrantType</c>).</summary>
public enum OAuthGrantType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The client credentials grant.</summary>
	[JsonStringEnumMemberName("client_credentials")]
	ClientCredentials
}
