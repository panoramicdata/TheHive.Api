using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Organisations;

/// <summary>
/// The body of a link-organizations request (the spec's <c>InputOrganisationLink</c>). Unset properties are omitted, and the server
/// then uses the <c>default</c> sharing profile for that direction; pass <c>new OrganisationLinkRequest()</c> (sent as <c>{}</c>) for both.
/// </summary>
public sealed class OrganisationLinkRequest
{
	/// <summary>The sharing profile applied when the current organization shares a case with the other one (1 to 64 characters); see <c>IOrganisations.ListSharingProfilesAsync</c>.</summary>
	[JsonPropertyName("linkType")]
	public string? LinkType { get; set; }

	/// <summary>The sharing profile applied when the other organization shares a case with the current one (1 to 64 characters).</summary>
	[JsonPropertyName("otherLinkType")]
	public string? OtherLinkType { get; set; }
}
