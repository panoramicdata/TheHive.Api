using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Shares;

/// <summary>The body of the share-a-task and share-an-observable requests (the spec's <c>InputCreateShare</c>).</summary>
public sealed class ShareOrganisationsRequest
{
	/// <summary>The names or IDs of the linked organizations to share with. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("organisations")]
	public List<string>? Organisations { get; set; }
}
