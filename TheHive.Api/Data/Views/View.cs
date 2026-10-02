using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Views;

/// <summary>A saved list view: filters, sort order, columns and display options (the spec's <c>OutputListView</c>).</summary>
public sealed class View
{
	/// <summary>The internal identifier (for example <c>~344112</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>ListView</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>When the view was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who created the view.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the view was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The login of the user who last updated the view, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

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
