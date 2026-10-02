using System.Text.Json.Serialization;
using TheHive.Api.Data.Organisations;

namespace TheHive.Api.Data.Users;

/// <summary>An organization a user belongs to, as listed in <see cref="User.Organisations"/> (the spec's <c>OutputOrganisationProfile</c>).</summary>
public sealed class UserOrganisationProfile
{
	/// <summary>The internal identifier of the organization (for example <c>~128458762</c>).</summary>
	[JsonPropertyName("organisationId")]
	public string OrganisationId { get; set; } = string.Empty;

	/// <summary>The name of the organization.</summary>
	[JsonPropertyName("organisation")]
	public string Organisation { get; set; } = string.Empty;

	/// <summary>The permission profile of the user in this organization.</summary>
	[JsonPropertyName("profile")]
	public string Profile { get; set; } = string.Empty;

	/// <summary>The relative path of the organization's avatar image, if it has one.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>The sharing links configured for the organization.</summary>
	[JsonPropertyName("links")]
	public List<OrganisationLink> Links { get; set; } = [];
}
