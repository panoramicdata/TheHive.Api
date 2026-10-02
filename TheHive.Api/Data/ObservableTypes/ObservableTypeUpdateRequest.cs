using System.Text.Json.Serialization;

namespace TheHive.Api.Data.ObservableTypes;

/// <summary>The body of an update-observable-type request (the spec's <c>InputUpdateObservableType</c>). Unset properties are omitted.</summary>
public sealed class ObservableTypeUpdateRequest
{
	/// <summary>When <see langword="false"/>, new observables of this type are lowercased on creation; existing ones are not affected.</summary>
	[JsonPropertyName("isCaseSensitive")]
	public bool? IsCaseSensitive { get; set; }
}
