using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Profiles;

/// <summary>The body of a create-profile request (the spec's <c>InputCreateProfile</c>). Unset properties are omitted.</summary>
public sealed class ProfileCreateRequest
{
	/// <summary>The kind of user the profile is for; when omitted, TheHive infers it from <see cref="Permissions"/>.</summary>
	[JsonPropertyName("type")]
	public ProfileType? Type { get; set; }

	/// <summary>The name of the profile (1 to 64 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The permission names to assign (each 1 to 64 characters); see <c>IPermissions.ListAsync</c>.</summary>
	[JsonPropertyName("permissions")]
	public List<string>? Permissions { get; set; }
}
