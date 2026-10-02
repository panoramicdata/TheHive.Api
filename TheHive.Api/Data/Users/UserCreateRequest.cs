using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>
/// The body of a create-user request (the spec's <c>InputCreateUser</c>). <see cref="Password"/> is a secret: it is sent in the JSON
/// body only, never in a URL, and this class does not print it (it is not a record and has no custom <c>ToString</c>).
/// </summary>
public sealed class UserCreateRequest
{
	/// <summary>The login used to sign in; it must be a valid email address (up to 128 characters).</summary>
	[JsonPropertyName("login")]
	public required string Login { get; set; }

	/// <summary>The display name (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The email address for notifications (up to 128 characters).</summary>
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	/// <summary>The initial password (1 to 128 characters). Secret: do not log it.</summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>The permission profile in the organization (1 to 64 characters); it must match an existing profile.</summary>
	[JsonPropertyName("profile")]
	public required string Profile { get; set; }

	/// <summary>The organization the user belongs to (1 to 128 characters); it must match an existing organization.</summary>
	[JsonPropertyName("organisation")]
	public string? Organisation { get; set; }

	/// <summary>The type of account.</summary>
	[JsonPropertyName("type")]
	public UserType? Type { get; set; }
}
