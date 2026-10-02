using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Attachments;

/// <summary>The body of an update-attachment request (the spec's <c>InputUpdateAttachment</c>). Unset properties keep their values.</summary>
public sealed class AttachmentUpdateRequest
{
	/// <summary>Whether external users can access the attachment through TheHive Portal (Platinum licence).</summary>
	[JsonPropertyName("external")]
	public bool? External { get; set; }
}
