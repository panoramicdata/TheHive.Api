using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>
/// OAuth 2.0 credentials of a mailbox (the spec's <c>InputEmailIntakeOAuth2</c> and <c>OutputEmailIntakeOAuth2</c>); required for Google
/// Workspace, Microsoft 365 and Microsoft Graph API (Platinum licence). The secrets (<see cref="Secret"/>, <see cref="AuthorizationCode"/>)
/// only ever travel in a request or response body, never a URL; the spec's output carries them too, and this client reports what the server returns.
/// </summary>
public sealed class EmailIntakeOAuth2
{
	/// <summary>The OAuth 2.0 client ID from the identity provider.</summary>
	[JsonPropertyName("clientId")]
	public string ClientId { get; set; } = string.Empty;

	/// <summary>The tenant ID from Microsoft Entra ID; required for Microsoft 365 and Microsoft Graph API.</summary>
	[JsonPropertyName("tenantId")]
	public string? TenantId { get; set; }

	/// <summary>SECRET. The OAuth 2.0 client secret from the identity provider.</summary>
	[JsonPropertyName("secret")]
	public string Secret { get; set; } = string.Empty;

	/// <summary>The OAuth 2.0 authority URL; defaults to the Microsoft identity platform endpoint for Microsoft providers.</summary>
	[JsonPropertyName("authority")]
	public string? Authority { get; set; }

	/// <summary>The OAuth 2.0 permission scopes the application needs.</summary>
	[JsonPropertyName("scopes")]
	public List<string>? Scopes { get; set; }

	/// <summary>The redirect URI registered in the OAuth 2.0 application; required for authorization code flows such as Google Workspace. Request only: the spec's output does not return it.</summary>
	[JsonPropertyName("redirectUri")]
	public string? RedirectUri { get; set; }

	/// <summary>SECRET. The authorization code from the OAuth 2.0 consent flow; Google Workspace only (see <see cref="Interfaces.IEmailIntake.SetAuthorizationCodeAsync"/>).</summary>
	[JsonPropertyName("authorizationCode")]
	public string? AuthorizationCode { get; set; }
}
