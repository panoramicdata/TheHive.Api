using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Attachments;

/// <summary>A file attached to a case, alert, task log, observable or organization (the spec's <c>OutputAttachment</c>).</summary>
public sealed class Attachment
{
	/// <summary>The internal identifier (for example <c>~456789012</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Attachment</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who uploaded the attachment.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the attachment, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the attachment was uploaded.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the attachment was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The file name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The cryptographic hashes of the file.</summary>
	[JsonPropertyName("hashes")]
	public List<string> Hashes { get; set; } = [];

	/// <summary>The file size in bytes.</summary>
	[JsonPropertyName("size")]
	public long Size { get; set; }

	/// <summary>The MIME content type of the file.</summary>
	[JsonPropertyName("contentType")]
	public string ContentType { get; set; } = string.Empty;

	/// <summary>The unique storage identifier of the file content (the wire's <c>id</c>, distinct from <see cref="Id"/>).</summary>
	[JsonPropertyName("id")]
	public string StorageId { get; set; } = string.Empty;

	/// <summary>The storage path of the file in the configured backend.</summary>
	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>Whether external users can access the attachment through TheHive Portal (Platinum licence).</summary>
	[JsonPropertyName("external")]
	public bool External { get; set; }
}
