using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Observables;

/// <summary>The body of an update-observable request (the spec's <c>InputUpdateObservable</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
/// <remarks><see cref="ObservableBulkUpdateRequest"/> derives from this class; passing a bulk request to <c>IObservables.UpdateAsync</c> would also send its <c>ids</c>.</remarks>
public class ObservableUpdateRequest
{
	/// <summary>The new observable type; it must match an existing observable type (1 to 64 characters).</summary>
	[JsonPropertyName("dataType")]
	public string? DataType { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown); set to <see langword="null"/> to unset it.</summary>
	[JsonPropertyName("message")]
	public Optional<string?> Message { get; set; }

	/// <summary>The new Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The new Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>Replaces all current tags with this set.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether the observable is an indicator of compromise (IOC).</summary>
	[JsonPropertyName("ioc")]
	public bool? Ioc { get; set; }

	/// <summary>Whether the observable has been sighted.</summary>
	[JsonPropertyName("sighted")]
	public bool? Sighted { get; set; }

	/// <summary>When the observable was last sighted; set to <see langword="null"/> to unset it.</summary>
	[JsonPropertyName("sightedAt")]
	public Optional<DateTimeOffset?> SightedAt { get; set; }

	/// <summary>Whether to exclude the observable from similarity checks.</summary>
	[JsonPropertyName("ignoreSimilarity")]
	public bool? IgnoreSimilarity { get; set; }

	/// <summary>Tags to add to the current set.</summary>
	[JsonPropertyName("addTags")]
	public List<string>? AddTags { get; set; }

	/// <summary>Tags to remove from the current set; absent tags are ignored.</summary>
	[JsonPropertyName("removeTags")]
	public List<string>? RemoveTags { get; set; }

	/// <summary>Whether external users can access the observable through TheHive Portal (Platinum licence).</summary>
	[JsonPropertyName("external")]
	public bool? External { get; set; }
}
