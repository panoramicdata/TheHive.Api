using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.ObservableTypes;

/// <summary>An observable type such as <c>ip</c> or <c>file</c> (the spec's <c>OutputObservableType</c>).</summary>
public sealed class ObservableType : TheHiveEntity
{
	/// <summary>The name of the type; it is the <c>dataType</c> value used when creating observables.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>Whether observables of this type require a file attachment instead of a text value.</summary>
	[JsonPropertyName("isAttachment")]
	public bool IsAttachment { get; set; }

	/// <summary>Whether values are kept as typed; when <see langword="false"/>, they are lowercased on creation.</summary>
	[JsonPropertyName("isCaseSensitive")]
	public bool IsCaseSensitive { get; set; }
}
