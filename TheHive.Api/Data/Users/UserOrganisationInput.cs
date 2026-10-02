using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>One organization membership in <see cref="UserOrganisationsSetRequest.Organisations"/> (the spec's <c>InputUserOrganisation</c>).</summary>
public sealed class UserOrganisationInput
{
	/// <summary>The organization name (1 to 128 characters); it must match an existing organization.</summary>
	[JsonPropertyName("organisation")]
	public required string Organisation { get; set; }

	/// <summary>The permission profile of the user in this organization (1 to 64 characters); it must match an existing profile.</summary>
	[JsonPropertyName("profile")]
	public required string Profile { get; set; }

	/// <summary>Whether this is the user's default organization. The server default is <see langword="false"/>.</summary>
	[JsonPropertyName("default")]
	public bool? Default { get; set; }
}
