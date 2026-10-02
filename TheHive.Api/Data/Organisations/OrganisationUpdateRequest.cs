using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Organisations;

/// <summary>The body of an update-organization request (the spec's <c>InputUpdateOrganisation</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class OrganisationUpdateRequest
{
	/// <summary>The new name (1 to 64 characters).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The new description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new default task-sharing rule for new case shares with this organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The new default observable-sharing rule for new case shares with this organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }

	/// <summary>Whether to lock or unlock the organization.</summary>
	[JsonPropertyName("locked")]
	public bool? Locked { get; set; }

	/// <summary>The new avatar image, as a Base64 string.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }
}
