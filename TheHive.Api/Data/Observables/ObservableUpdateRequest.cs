using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Observables;

/// <summary>The body of an update-observable request (the spec's <c>InputUpdateObservable</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
/// <remarks><see cref="ObservableBulkUpdateRequest"/> derives from this class; passing a bulk request to <c>IObservables.UpdateAsync</c> would also send its <c>ids</c>.</remarks>
public class ObservableUpdateRequest : ObservableCoreBase
{
	/// <summary>The new observable type; it must match an existing observable type (1 to 64 characters).</summary>
	[JsonPropertyName("dataType")]
	[JsonPropertyOrder(1)]
	public string? DataType { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown); set to <see langword="null"/> to unset it.</summary>
	[JsonPropertyName("message")]
	[JsonPropertyOrder(2)]
	public Optional<string?> Message { get; set; }

	/// <summary>When the observable was last sighted; set to <see langword="null"/> to unset it.</summary>
	[JsonPropertyName("sightedAt")]
	[JsonPropertyOrder(510)]
	public Optional<DateTimeOffset?> SightedAt { get; set; }

	/// <summary>Whether to exclude the observable from similarity checks.</summary>
	[JsonPropertyName("ignoreSimilarity")]
	[JsonPropertyOrder(520)]
	public bool? IgnoreSimilarity { get; set; }

	/// <summary>Tags to add to the current set.</summary>
	[JsonPropertyName("addTags")]
	[JsonPropertyOrder(530)]
	public List<string>? AddTags { get; set; }

	/// <summary>Tags to remove from the current set; absent tags are ignored.</summary>
	[JsonPropertyName("removeTags")]
	[JsonPropertyOrder(540)]
	public List<string>? RemoveTags { get; set; }

	/// <summary>Whether external users can access the observable through TheHive Portal (Platinum licence).</summary>
	[JsonPropertyName("external")]
	[JsonPropertyOrder(550)]
	public bool? External { get; set; }
}
