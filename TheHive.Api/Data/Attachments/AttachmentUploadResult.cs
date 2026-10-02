using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Attachments;

/// <summary>The attachments created by an upload (the spec's <c>OutputAttachments</c>).</summary>
public sealed class AttachmentUploadResult
{
	/// <summary>The attachments created, one per uploaded file.</summary>
	[JsonPropertyName("attachments")]
	public List<Attachment> Attachments { get; set; } = [];
}
