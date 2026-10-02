using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Users;

/// <summary>The body of a set-user-organizations request (the spec's <c>InputSetUserOrganisations</c>).</summary>
public sealed class UserOrganisationsSetRequest
{
	/// <summary>The complete list of memberships; it replaces all existing ones. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("organisations")]
	public List<UserOrganisationInput>? Organisations { get; set; }
}
