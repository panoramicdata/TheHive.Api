using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Shares;

/// <summary>The body of the unshare requests for a case, task or observable (the spec's <c>InputRemoveShares</c>).</summary>
public sealed class ShareRemoveRequest
{
	/// <summary>The names or IDs of the organizations to remove from sharing. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("organisations")]
	public List<string>? Organisations { get; set; }
}
