using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Profiles;

/// <summary>The body of an update-profile request (the spec's <c>InputUpdateProfile</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class ProfileUpdateRequest
{
	/// <summary>The new name (1 to 64 characters).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The permission names that replace all current permissions; see <c>IPermissions.ListAsync</c>.</summary>
	[JsonPropertyName("permissions")]
	public List<string>? Permissions { get; set; }
}
