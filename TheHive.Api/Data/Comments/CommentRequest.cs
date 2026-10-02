using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Comments;

/// <summary>The body of an add-comment or update-comment request (the spec's <c>InputComment</c>, used by both). Unset properties are omitted.</summary>
public sealed class CommentRequest
{
	/// <summary>The text of the comment (at most 1048576 characters).</summary>
	[JsonPropertyName("message")]
	public required string Message { get; set; }

	/// <summary>
	/// Whether the comment is visible to external users through TheHive Portal (Platinum licence, case must be shared through the portal).
	/// Has no effect on alert comments, which are always internal. The server default is <see langword="false"/>.
	/// </summary>
	[JsonPropertyName("external")]
	public bool? External { get; set; }
}
