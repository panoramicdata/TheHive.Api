using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Observables;

/// <summary>An observable to create, nested in a create request such as <c>AlertCreateRequest.Observables</c> (the spec's <c>InputCreateObservable</c>). Unset properties are omitted.</summary>
/// <remarks>The spec lets <c>data</c> be a string or an array of strings, and <c>attachment</c> a multipart part name or an existing-attachment object (or arrays of either). This model always sends the array form of <c>data</c> and the object-array form of <c>attachment</c>; part names only make sense in a multipart request, which is not modelled.</remarks>
public sealed class ObservableInput : ObservableCoreBase
{
	/// <summary>The observable type, such as <c>ip</c> or <c>file</c> (1 to 64 characters).</summary>
	[JsonPropertyName("dataType")]
	[JsonPropertyOrder(1)]
	public required string DataType { get; set; }

	/// <summary>The observable values, each up to 4096 characters; one observable is created per value. Omit for attachment types.</summary>
	[JsonPropertyName("data")]
	[JsonPropertyOrder(2)]
	public List<string>? Data { get; set; }

	/// <summary>A note or context about the observable (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("message")]
	[JsonPropertyOrder(3)]
	public string? Message { get; set; }

	/// <summary>Deprecated by the spec: maps to the creation date. Do not use.</summary>
	[JsonPropertyName("startDate")]
	[JsonPropertyOrder(4)]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>Existing attachments to use as the observable's files.</summary>
	[JsonPropertyName("attachment")]
	[JsonPropertyOrder(5)]
	public List<ObservableAttachmentReference>? Attachment { get; set; }

	/// <summary>When the observable was sighted.</summary>
	[JsonPropertyName("sightedAt")]
	[JsonPropertyOrder(510)]
	public DateTimeOffset? SightedAt { get; set; }

	/// <summary>Whether to exclude the observable from similarity checks.</summary>
	[JsonPropertyName("ignoreSimilarity")]
	[JsonPropertyOrder(520)]
	public bool? IgnoreSimilarity { get; set; }

	/// <summary>Whether the attachment is a zip archive that TheHive unpacks using <see cref="ZipPassword"/>.</summary>
	[JsonPropertyName("isZip")]
	[JsonPropertyOrder(530)]
	public bool? IsZip { get; set; }

	/// <summary>The password that decrypts the zip archive when <see cref="IsZip"/> is <see langword="true"/> (1 to 512 characters).</summary>
	[JsonPropertyName("zipPassword")]
	[JsonPropertyOrder(540)]
	public string? ZipPassword { get; set; }
}
