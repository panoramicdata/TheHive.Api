using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Attachments;

/// <summary>A file attached to a case, alert, task log, observable or organization (the spec's <c>OutputAttachment</c>).</summary>
public sealed class Attachment : TheHiveEntity
{
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

	/// <summary>The unique storage identifier of the file content (the wire's <c>id</c>, distinct from <see cref="TheHive.Api.Data.Common.TheHiveAuditedEntity.Id"/>).</summary>
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
