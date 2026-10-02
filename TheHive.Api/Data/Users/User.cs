using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>
/// A user account (the spec's <c>OutputUser</c>). It carries no secrets: the API key, password and MFA secret are never returned,
/// only <see cref="HasKey"/>, <see cref="HasPassword"/> and <see cref="HasMfa"/>. The spec's output has no <c>_type</c> property.
/// </summary>
public sealed class User
{
	/// <summary>The internal identifier (for example <c>~192024</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The login of the user who created the account.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the account, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the account was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the account was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login, always a valid email address; it can be used instead of <see cref="Id"/>.</summary>
	[JsonPropertyName("login")]
	public string Login { get; set; } = string.Empty;

	/// <summary>The display name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The email address used for notifications, if set.</summary>
	[JsonPropertyName("email")]
	public string? Email { get; set; }

	/// <summary>Whether the account has an API key (the key itself is never returned here).</summary>
	[JsonPropertyName("hasKey")]
	public bool HasKey { get; set; }

	/// <summary>Whether the account has a password set.</summary>
	[JsonPropertyName("hasPassword")]
	public bool HasPassword { get; set; }

	/// <summary>Whether multi-factor authentication is enabled for the account.</summary>
	[JsonPropertyName("hasMFA")]
	public bool HasMfa { get; set; }

	/// <summary>Whether the account is locked; a locked account cannot sign in or use the API.</summary>
	[JsonPropertyName("locked")]
	public bool Locked { get; set; }

	/// <summary>The permission profile of the user in the current organization.</summary>
	[JsonPropertyName("profile")]
	public string Profile { get; set; } = string.Empty;

	/// <summary>The permissions granted to the user in the current organization.</summary>
	[JsonPropertyName("permissions")]
	public List<string> Permissions { get; set; } = [];

	/// <summary>The name of the organization in whose context the user was retrieved.</summary>
	[JsonPropertyName("organisation")]
	public string Organisation { get; set; } = string.Empty;

	/// <summary>The relative path of the avatar image (for example <c>api/v1/user/~1048576/avatar/e3b0c44298fc1c14</c>), if the user has one; pass its last segment to <c>IUsers.GetAvatarAsync</c>.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>The organizations the user belongs to, with the profile and avatar used in each.</summary>
	[JsonPropertyName("organisations")]
	public List<UserOrganisationProfile> Organisations { get; set; } = [];

	/// <summary>The type of account.</summary>
	[JsonPropertyName("type")]
	public UserType Type { get; set; }

	/// <summary>The default organization used when the user signs in, if set.</summary>
	[JsonPropertyName("defaultOrganisation")]
	public string? DefaultOrganisation { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
