using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Organisations;

/// <summary>A link to another organization with that organization's details (the spec's <c>OutputOrganisationLink</c>).</summary>
public sealed class OrganisationLinkDetails
{
	/// <summary>The sharing profile applied when the current organization shares a case with the linked one.</summary>
	[JsonPropertyName("linkType")]
	public string LinkType { get; set; } = string.Empty;

	/// <summary>The sharing profile applied when the linked organization shares a case with the current one.</summary>
	[JsonPropertyName("otherLinkType")]
	public string OtherLinkType { get; set; } = string.Empty;

	/// <summary>The linked organization.</summary>
	[JsonPropertyName("organisation")]
	public Organisation Organisation { get; set; } = new();
}
