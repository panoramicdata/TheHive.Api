using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Permissions;

/// <summary>A permission that can be assigned to a profile (the spec's <c>PermissionDesc</c>).</summary>
public sealed class PermissionDescription
{
	/// <summary>
	/// The permission name (for example <c>manageCase/create</c>), the value to put in a profile's permission list.
	/// The spec enumerates the names, but the set grows with TheHive releases, so it is kept as a string rather than a tolerant enum
	/// that would turn a new permission into <c>Unknown</c>; profile permissions are strings too.
	/// </summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The human-readable label (for example <c>Create cases</c>).</summary>
	[JsonPropertyName("label")]
	public string Label { get; set; } = string.Empty;

	/// <summary>Whether the permission consumes a licence.</summary>
	[JsonPropertyName("consumesLicense")]
	public bool ConsumesLicense { get; set; }

	/// <summary>The scopes where the permission applies (for example <c>organisation</c>).</summary>
	[JsonPropertyName("scope")]
	public List<string> Scope { get; set; } = [];
}
