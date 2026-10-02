using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Shares;

/// <summary>The body of an update-share request (the spec's <c>InputUpdateShare</c>).</summary>
public sealed class ShareUpdateRequest
{
	/// <summary>The profile (1 to 64 characters) defining the permissions of the organization's members on the case; it must match an existing profile.</summary>
	[JsonPropertyName("profile")]
	public required string Profile { get; set; }
}
