using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Views;

/// <summary>The display options saved with a view (the spec's <c>ListOptions</c>), used in requests and responses.</summary>
public sealed class ViewListOptions
{
	/// <summary>The number of rows shown per page: 10, 30, 50, 100, 150 or 300. The spec requires it, so it is always sent; the default is 50 (the spec example).</summary>
	[JsonPropertyName("itemsPerPage")]
	public int ItemsPerPage { get; set; } = 50;

	/// <summary>Whether the list groups its items by their group value instead of showing a flat list (only for lists that support grouping, such as tasks).</summary>
	[JsonPropertyName("listAsGroup")]
	public bool ListAsGroup { get; set; }

	/// <summary>Whether the list refreshes automatically when new activity occurs.</summary>
	[JsonPropertyName("autoRefresh")]
	public bool AutoRefresh { get; set; }

	/// <summary>Whether the statistics panel above the list is expanded.</summary>
	[JsonPropertyName("statsIsOpen")]
	public bool StatsIsOpen { get; set; }
}
