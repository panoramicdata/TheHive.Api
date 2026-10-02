using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>An organization membership of a user, as returned by <c>IUsers.SetOrganisationsAsync</c> (the spec's <c>OutputUserOrganisation</c>).</summary>
public sealed class UserOrganisation
{
	/// <summary>The organization name.</summary>
	[JsonPropertyName("organisation")]
	public string Organisation { get; set; } = string.Empty;

	/// <summary>The permission profile of the user in this organization.</summary>
	[JsonPropertyName("profile")]
	public string Profile { get; set; } = string.Empty;

	/// <summary>Whether this is the user's default organization.</summary>
	[JsonPropertyName("default")]
	public bool Default { get; set; }
}
