using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>
/// The authentication an alert feeder uses towards the external system. It models the spec's polymorphic authentication schemas
/// (the create, update and output variants of the <c>basic</c>, <c>bearer</c>, <c>key</c>, <c>none</c> and <c>oauth2</c> kinds,
/// discriminated by <c>type</c>) as one flattened class, like <c>Access</c>. Set <see cref="Type"/> (see <see cref="AlertFeederAuthTypes"/>)
/// and the properties of that kind; the others stay <see langword="null"/> and are omitted.
/// </summary>
/// <remarks>
/// <see cref="Type"/> is a string, not an enum, and members this class does not model are kept in <see cref="AdditionalProperties"/>,
/// so an authentication kind that a newer server adds is read and written back unchanged. Which members apply to which kind:
/// <c>basic</c> uses <see cref="Username"/> and <see cref="Password"/>; <c>bearer</c> uses <see cref="Key"/>; <c>key</c> uses
/// <see cref="Key"/> and <see cref="Prefix"/>; <c>oauth2</c> uses the client and token members. The secrets
/// (<see cref="Password"/>, <see cref="Key"/>, <see cref="ClientSecret"/>) only ever travel in a request or response body, never a URL. The spec's
/// output variants carry <c>password</c> and <c>key</c> too (this client does not mask anything: it reports what the server returns) but not
/// the OAuth 2.0 <c>clientSecret</c>, and its update variant leaves <c>clientSecret</c> optional. The class has no <c>ToString</c> override, so
/// nothing prints the secrets. Only the discriminator is protected against values this client does not know: the nested tolerant enums
/// (<see cref="OAuthGrantType"/>, <see cref="OAuthClientAuthenticationMethod"/>) write <c>Unknown</c> when the server sent a value this client
/// does not recognise, so a replacing update built from a read model can corrupt such a field. Do not round-trip a read model blindly.
/// </remarks>
public sealed class AlertFeederAuth
{
	/// <summary>The authentication kind: one of the <see cref="AlertFeederAuthTypes"/> values, or a kind this client does not know.</summary>
	[JsonPropertyName("type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The user name (1 to 128 characters); <c>basic</c> only.</summary>
	[JsonPropertyName("username")]
	public string? Username { get; set; }

	/// <summary>SECRET. The password (1 to 512 characters); <c>basic</c> only.</summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>SECRET. The token or API key (1 to 8192 characters); <c>bearer</c> and <c>key</c> only.</summary>
	[JsonPropertyName("key")]
	public string? Key { get; set; }

	/// <summary>The prefix put before the key (1 to 32 characters); <c>key</c> only.</summary>
	[JsonPropertyName("prefix")]
	public string? Prefix { get; set; }

	/// <summary>The OAuth 2.0 client ID (1 to 512 characters); <c>oauth2</c> only.</summary>
	[JsonPropertyName("clientId")]
	public string? ClientId { get; set; }

	/// <summary>SECRET. The OAuth 2.0 client secret (1 to 512 characters); <c>oauth2</c> only. Required when creating; optional when updating; never returned by the spec's output schema.</summary>
	[JsonPropertyName("clientSecret")]
	public string? ClientSecret { get; set; }

	/// <summary>The OAuth 2.0 grant type; <c>oauth2</c> only.</summary>
	[JsonPropertyName("grantType")]
	public OAuthGrantType? GrantType { get; set; }

	/// <summary>The OAuth 2.0 token endpoint URL; <c>oauth2</c> only.</summary>
	[JsonPropertyName("tokenUrl")]
	public string? TokenUrl { get; set; }

	/// <summary>The OAuth 2.0 scopes to request; <c>oauth2</c> only.</summary>
	[JsonPropertyName("scope")]
	public List<string>? Scope { get; set; }

	/// <summary>How the client authenticates to the token endpoint; <c>oauth2</c> only.</summary>
	[JsonPropertyName("clientAuthenticationMethod")]
	public OAuthClientAuthenticationMethod? ClientAuthenticationMethod { get; set; }

	/// <summary>Additional parameters for the token request; <c>oauth2</c> only.</summary>
	[JsonPropertyName("tokenParameters")]
	public List<OAuthTokenParameter>? TokenParameters { get; set; }

	/// <summary>Members this class does not model (for example those of an authentication kind added by a newer server), preserved when read and written back.</summary>
	[JsonExtensionData]
	public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
}
