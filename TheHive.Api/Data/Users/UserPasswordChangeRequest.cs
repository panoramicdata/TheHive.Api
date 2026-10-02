using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>
/// The body of a change-password request (the spec's <c>InputChangeUserPassword</c>). Both passwords are secrets: they are sent in
/// the JSON body only and this class does not print them.
/// </summary>
public sealed class UserPasswordChangeRequest
{
	/// <summary>The new password (1 to 128 characters). Secret: do not log it.</summary>
	[JsonPropertyName("password")]
	public required string Password { get; set; }

	/// <summary>The current password (1 to 128 characters). Secret: do not log it.</summary>
	[JsonPropertyName("currentPassword")]
	public required string CurrentPassword { get; set; }
}
