using System.Text.Json.Serialization;

namespace TheHive.Api.Data.ObservableTypes;

/// <summary>The body of a create-observable-type request (the spec's <c>InputObservableType</c>). Unset properties are omitted.</summary>
public sealed class ObservableTypeCreateRequest
{
	/// <summary>The name of the type (1 to 64 characters); once created it is a valid <c>dataType</c> for observables.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>Whether observables of this type require a file upload instead of a text value.</summary>
	[JsonPropertyName("isAttachment")]
	public bool? IsAttachment { get; set; }

	/// <summary>When <see langword="false"/>, new observables of this type are lowercased on creation; existing ones are not affected.</summary>
	[JsonPropertyName("isCaseSensitive")]
	public bool? IsCaseSensitive { get; set; }
}
