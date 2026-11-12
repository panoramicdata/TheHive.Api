using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Profiles;

/// <summary>A permission profile (the spec's <c>OutputProfile</c>).</summary>
public sealed class Profile : TheHiveAuditedEntity
{
	/// <summary>The entity type, always <c>profile</c>.</summary>
	[JsonPropertyName("_type")]
	public string EntityType { get; set; } = string.Empty;

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
