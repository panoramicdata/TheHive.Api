using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of a create-case request (the spec's <c>InputCreateCase</c>). Unset properties are omitted.</summary>
public sealed class CaseCreateRequest : CaseRequestBase
{
	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>Access-control settings; the organization's default is used when not given.</summary>
	[JsonPropertyName("access")]
	public Access? Access { get; set; }
}
