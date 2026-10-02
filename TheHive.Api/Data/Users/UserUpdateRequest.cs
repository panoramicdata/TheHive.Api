using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Users;

/// <summary>The body of an update-user request (the spec's <c>InputUpdateUser</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class UserUpdateRequest
{
	/// <summary>The new display name (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The organization context for a <see cref="Profile"/> change; the user must belong to it.</summary>
	[JsonPropertyName("organisation")]
	public string? Organisation { get; set; }

	/// <summary>The new permission profile in the organization named by <see cref="Organisation"/>.</summary>
	[JsonPropertyName("profile")]
	public string? Profile { get; set; }

	/// <summary>Whether to lock or unlock the account (requires <c>manageUser</c>).</summary>
	[JsonPropertyName("locked")]
	public bool? Locked { get; set; }

	/// <summary>The new avatar as a raw Base64 string (always stored as JPEG); set to <see langword="null"/> or an empty string to remove the current avatar.</summary>
	[JsonPropertyName("avatar")]
	public Optional<string?> Avatar { get; set; }

	/// <summary>The new notification email address (up to 128 characters); set to <see langword="null"/> to remove it.</summary>
	[JsonPropertyName("email")]
	public Optional<string?> Email { get; set; }

	/// <summary>The new default organization; the user must already belong to it.</summary>
	[JsonPropertyName("defaultOrganisation")]
	public string? DefaultOrganisation { get; set; }

	/// <summary>The new account type; it cannot be changed to or from <see cref="UserType.External"/>.</summary>
	[JsonPropertyName("type")]
	public UserType? Type { get; set; }
}
