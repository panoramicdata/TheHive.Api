using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Observables;

/// <summary>An observable to create, nested in a create request such as <c>AlertCreateRequest.Observables</c> (the spec's <c>InputCreateObservable</c>). Unset properties are omitted.</summary>
/// <remarks>The spec lets <c>data</c> be a string or an array of strings, and <c>attachment</c> a multipart part name or an existing-attachment object (or arrays of either). This model always sends the array form of <c>data</c> and the object-array form of <c>attachment</c>; part names only make sense in a multipart request, which is not modelled.</remarks>
public sealed class ObservableInput
{
	/// <summary>The observable type, such as <c>ip</c> or <c>file</c> (1 to 64 characters).</summary>
	[JsonPropertyName("dataType")]
	public required string DataType { get; set; }

	/// <summary>The observable values, each up to 4096 characters; one observable is created per value. Omit for attachment types.</summary>
	[JsonPropertyName("data")]
	public List<string>? Data { get; set; }

	/// <summary>A note or context about the observable (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("message")]
	public string? Message { get; set; }

	/// <summary>Deprecated by the spec: maps to the creation date. Do not use.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>Existing attachments to use as the observable's files.</summary>
	[JsonPropertyName("attachment")]
	public List<ObservableAttachmentReference>? Attachment { get; set; }

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>. The server default is 2.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>. The server default is 2.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether the observable is an indicator of compromise (IOC).</summary>
	[JsonPropertyName("ioc")]
	public bool? Ioc { get; set; }

	/// <summary>Whether the observable has been sighted.</summary>
	[JsonPropertyName("sighted")]
	public bool? Sighted { get; set; }

	/// <summary>When the observable was sighted.</summary>
	[JsonPropertyName("sightedAt")]
	public DateTimeOffset? SightedAt { get; set; }

	/// <summary>Whether to exclude the observable from similarity checks.</summary>
	[JsonPropertyName("ignoreSimilarity")]
	public bool? IgnoreSimilarity { get; set; }

	/// <summary>Whether the attachment is a zip archive that TheHive unpacks using <see cref="ZipPassword"/>.</summary>
	[JsonPropertyName("isZip")]
	public bool? IsZip { get; set; }

	/// <summary>The password that decrypts the zip archive when <see cref="IsZip"/> is <see langword="true"/> (1 to 512 characters).</summary>
	[JsonPropertyName("zipPassword")]
	public string? ZipPassword { get; set; }
}
