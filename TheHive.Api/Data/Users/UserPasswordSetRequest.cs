using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>
/// The body of a set-password request (the spec's <c>InputSetUserPassword</c>). The password is a secret: it is sent in the JSON
/// body only and this class does not print it.
/// </summary>
public sealed class UserPasswordSetRequest
{
	/// <summary>The new password (1 to 128 characters). Secret: do not log it.</summary>
	[JsonPropertyName("password")]
	public required string Password { get; set; }
}
