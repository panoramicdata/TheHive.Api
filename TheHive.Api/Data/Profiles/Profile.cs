using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Profiles;

/// <summary>A permission profile (the spec's <c>OutputProfile</c>).</summary>
public sealed class Profile
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>profile</c>.</summary>
	[JsonPropertyName("_type")]
	public string EntityType { get; set; } = string.Empty;

	/// <summary>The login of the user who created the profile.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the profile, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the profile was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the profile was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The kind of user the profile is for.</summary>
	[JsonPropertyName("type")]
	public ProfileType Type { get; set; }

	/// <summary>The name of the profile.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The permission names assigned to the profile (for example <c>manageCase/create</c>); see <c>IPermissions.ListAsync</c>.</summary>
	[JsonPropertyName("permissions")]
	public List<string> Permissions { get; set; } = [];

	/// <summary>Whether the profile can be modified.</summary>
	[JsonPropertyName("editable")]
	public bool Editable { get; set; }

	/// <summary>Whether the profile can be assigned in the admin organization.</summary>
	[JsonPropertyName("forAdmin")]
	public bool ForAdmin { get; set; }

	/// <summary>Whether the profile can be assigned in non-admin organizations.</summary>
	[JsonPropertyName("forOrg")]
	public bool ForOrg { get; set; }

	/// <summary>Whether the profile can be assigned to external users of TheHive Portal.</summary>
	[JsonPropertyName("forExternal")]
	public bool ForExternal { get; set; }

	/// <summary>Whether assigning the profile to a user consumes a licence.</summary>
	[JsonPropertyName("consumesLicense")]
	public bool ConsumesLicense { get; set; }
}
