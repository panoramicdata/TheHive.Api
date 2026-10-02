using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Shares;

/// <summary>
/// The body of the share-a-case and set-case-shares requests (the spec's <c>InputCreateShares</c>). Each entry is a
/// <see cref="ShareSettings"/> (the spec's <c>InputShare</c>).
/// </summary>
public sealed class ShareCreateRequest
{
	/// <summary>The sharing configurations, one per organization. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("shares")]
	public List<ShareSettings>? Shares { get; set; }
}
