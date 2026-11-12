using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Views;

/// <summary>A saved list view: filters, sort order, columns and display options (the spec's <c>OutputListView</c>).</summary>
public sealed class View : TheHiveEntity
{
	/// <summary>The name of the view.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The list the view applies to (the spec declares a plain string here, documented as the same value set as the create request).</summary>
	[JsonPropertyName("entity")]
	public ViewEntity Entity { get; set; }

	/// <summary>The filter applied to the list, as a JSON object using the operators of the Query API <c>filter</c> step; <c>ValueKind</c> is <c>Undefined</c> if a response omits it.</summary>
	[JsonPropertyName("filter")]
	public JsonElement Filter { get; set; }

	/// <summary>The saved display options.</summary>
	[JsonPropertyName("listOptions")]
	public ViewListOptions ListOptions { get; set; } = new();

	/// <summary>The saved sort order: field names prefixed with <c>+</c> (ascending) or <c>-</c> (descending).</summary>
	[JsonPropertyName("sortList")]
	public List<string> SortList { get; set; } = [];

	/// <summary>The optional columns displayed on top of the fixed set for the entity type.</summary>
	[JsonPropertyName("showColumns")]
	public List<string> ShowColumns { get; set; } = [];

	/// <summary>Whether the view is shared with the whole organization (otherwise it is private to its creator).</summary>
	[JsonPropertyName("isShared")]
	public bool IsShared { get; set; }
}
