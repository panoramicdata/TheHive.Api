using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Organisations;

/// <summary>
/// A sharing link from an organization to another (the spec's <c>OrganisationLink</c>), as returned in <see cref="Organisation.Links"/>
/// and sent in <see cref="OrganisationBulkLinkRequest.Links"/>. Unset optional properties are omitted. The three spec-required members use <c>required</c> deliberately; <c>TheHiveJson.Options</c> ignores it on read, so a response missing one still deserializes.
/// </summary>
public sealed class OrganisationLink
{
	/// <summary>The ID or name of the linked organization.</summary>
	[JsonPropertyName("toOrganisation")]
	public required string ToOrganisation { get; set; }

	/// <summary>The Base64-encoded avatar of the linked organization; read from responses, not needed in requests.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>The sharing profile applied when the owning organization shares a case with the linked one (1 to 64 characters); see <c>IOrganisations.ListSharingProfilesAsync</c>.</summary>
	[JsonPropertyName("linkType")]
	public required string LinkType { get; set; }

	/// <summary>The sharing profile applied when the linked organization shares a case with the owning one (1 to 64 characters).</summary>
	[JsonPropertyName("otherLinkType")]
	public required string OtherLinkType { get; set; }
}
