using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>The memberships a user has after <c>IUsers.SetOrganisationsAsync</c> (the spec's <c>OutputSetUserOrganisations</c>).</summary>
public sealed class UserOrganisationsResult
{
	/// <summary>The organizations the user now belongs to.</summary>
	[JsonPropertyName("organisations")]
	public List<UserOrganisation> Organisations { get; set; } = [];
}
