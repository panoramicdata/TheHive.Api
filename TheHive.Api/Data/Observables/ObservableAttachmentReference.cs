using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Observables;

/// <summary>A reference to an attachment already stored in TheHive, used as an observable's file (the object form of the spec's <c>InputCreateObservable.attachment</c>).</summary>
public sealed class ObservableAttachmentReference
{
	/// <summary>The file name of the attachment as stored in TheHive (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The MIME content type of the attachment (1 to 128 characters).</summary>
	[JsonPropertyName("contentType")]
	public required string ContentType { get; set; }

	/// <summary>The ID of the attachment in TheHive (1 to 128 characters).</summary>
	[JsonPropertyName("id")]
	public required string Id { get; set; }

	/// <summary>Whether the attachment is accessible to external users through TheHive Portal.</summary>
	[JsonPropertyName("external")]
	public bool? External { get; set; }
}
