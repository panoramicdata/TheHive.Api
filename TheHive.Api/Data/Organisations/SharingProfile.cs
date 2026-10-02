using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Organisations;

/// <summary>A sharing profile: the defaults applied when an organization shares a case with another (the spec's <c>OutputSharingProfile</c>).</summary>
public sealed class SharingProfile
{
	/// <summary>The unique name of the profile (for example <c>default</c>); the value used for <see cref="OrganisationLink.LinkType"/>.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>Whether cases owned by the organization are shared automatically when this profile is applied.</summary>
	[JsonPropertyName("autoShare")]
	public bool AutoShare { get; set; }

	/// <summary>Whether the sharing rules of this profile can be overridden per case.</summary>
	[JsonPropertyName("editable")]
	public bool Editable { get; set; }

	/// <summary>The permission profile applied to members of the linked organization.</summary>
	[JsonPropertyName("permissionProfile")]
	public string PermissionProfile { get; set; } = string.Empty;

	/// <summary>The task-sharing rule.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule TaskRule { get; set; }

	/// <summary>The observable-sharing rule.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule ObservableRule { get; set; }
}
