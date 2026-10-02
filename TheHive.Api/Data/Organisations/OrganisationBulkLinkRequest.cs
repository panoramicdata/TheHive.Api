using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Organisations;

/// <summary>The body of a replace-all-links request (the spec's <c>InputOrganisationBulkLink</c>).</summary>
public sealed class OrganisationBulkLinkRequest
{
	/// <summary>The complete set of sharing links; it replaces every existing link, and an empty list removes them all. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("links")]
	public List<OrganisationLink>? Links { get; set; }
}
