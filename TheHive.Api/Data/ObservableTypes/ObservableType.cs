using System.Text.Json.Serialization;

namespace TheHive.Api.Data.ObservableTypes;

/// <summary>An observable type such as <c>ip</c> or <c>file</c> (the spec's <c>OutputObservableType</c>).</summary>
public sealed class ObservableType
{
	/// <summary>The internal identifier (for example <c>~4096</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>ObservableType</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>When the type was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login of the user who last updated the type, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the type was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who created the type.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

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
