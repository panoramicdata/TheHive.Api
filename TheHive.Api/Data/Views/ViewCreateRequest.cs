using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Views;

/// <summary>A view to create (the spec's <c>InputCreateListView</c>). The request body must not exceed 1 MB. Unset optional properties are omitted.</summary>
public sealed class ViewCreateRequest
{
	/// <summary>The name of the view.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The list the view applies to.</summary>
	[JsonPropertyName("entity")]
	public required ViewEntity Entity { get; set; }

	/// <summary>The filter applied to the list, as a JSON object using the operators of the Query API <c>filter</c> step, for example <c>{"_and":[{"_eq":{"_field":"status","_value":"InProgress"}}]}</c>; use <c>{}</c> for no filter.</summary>
	/// <remarks>A default (<c>Undefined</c>) <see cref="JsonElement"/> throws <see cref="InvalidOperationException"/> when serialised: supply a real JSON object.</remarks>
	[JsonPropertyName("filter")]
	public required JsonElement Filter { get; set; }

	/// <summary>The display options to save.</summary>
	[JsonPropertyName("listOptions")]
	public required ViewListOptions ListOptions { get; set; }

	/// <summary>The sort order: field names prefixed with <c>+</c> (ascending) or <c>-</c> (descending); at most 15.</summary>
	[JsonPropertyName("sortList")]
	public List<string>? SortList { get; set; }

	/// <summary>The optional columns displayed on top of the fixed set for the entity type; at most 15, unique.</summary>
	[JsonPropertyName("showColumns")]
	public List<string>? ShowColumns { get; set; }

	/// <summary>Whether the view is shared with the whole organization (every member can then use, modify and delete it) rather than private.</summary>
	[JsonPropertyName("isShared")]
	public required bool IsShared { get; set; }
}
