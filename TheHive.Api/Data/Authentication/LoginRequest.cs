using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Authentication;

/// <summary>
/// The body of a login request (the spec's <c>LoginInput</c>). <see cref="Password"/> and <see cref="Code"/> are secrets: they are sent in the JSON
/// body only and this class does not print them. Unset properties are omitted.
/// </summary>
public sealed class LoginRequest
{
	/// <summary>The login to sign in with, typically an email address; a plain user name is accepted when a default domain is configured.</summary>
	[JsonPropertyName("user")]
	public required string User { get; set; }

	/// <summary>The password of <see cref="User"/>. SECRET: do not log it.</summary>
	[JsonPropertyName("password")]
	public required string Password { get; set; }

	/// <summary>The ID or name of the organization to log into; it must be one the user belongs to and that is not locked. When unset, TheHive uses the default organization.</summary>
	[JsonPropertyName("organisation")]
	public string? Organisation { get; set; }

	/// <summary>The current code from the authenticator app; required when multifactor authentication is enabled for the user. SECRET: do not log it.</summary>
	[JsonPropertyName("code")]
	public string? Code { get; set; }
}
