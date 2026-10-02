using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Views;

/// <summary>The body of an update-view request (the spec's <c>InputUpdateListView</c>). Only set properties are sent; the rest keep their values. The spec has no clearable fields.</summary>
public sealed class ViewUpdateRequest
{
	/// <summary>The new name.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The new filter, as a JSON object using the operators of the Query API <c>filter</c> step.</summary>
	/// <remarks>A default (<c>Undefined</c>) <see cref="JsonElement"/> throws <see cref="InvalidOperationException"/> when serialised; leave the property <see langword="null"/> to keep the current filter.</remarks>
	[JsonPropertyName("filter")]
	public JsonElement? Filter { get; set; }

	/// <summary>The new display options.</summary>
	[JsonPropertyName("listOptions")]
	public ViewListOptions? ListOptions { get; set; }

	/// <summary>The new sort order: field names prefixed with <c>+</c> (ascending) or <c>-</c> (descending); at most 15.</summary>
	[JsonPropertyName("sortList")]
	public List<string>? SortList { get; set; }

	/// <summary>The new optional columns; at most 15, unique.</summary>
	[JsonPropertyName("showColumns")]
	public List<string>? ShowColumns { get; set; }

	/// <summary>Whether to share the view with the whole organization or make it private.</summary>
	[JsonPropertyName("isShared")]
	public bool? IsShared { get; set; }
}
