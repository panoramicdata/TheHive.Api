using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>
/// A widget in a case report template: one self-contained section of the report body. The spec models it as a <c>oneOf</c> of fifteen
/// schemas discriminated by <c>_kind</c> (<c>Widget</c>); this one flattened class carries the members of all of them, like the
/// connector unions. Set <see cref="Kind"/> (see <see cref="CaseReportWidgetKinds"/>) and only the members that kind uses; unset (null)
/// members are omitted. Members this client does not model (a newer server) are kept in <see cref="AdditionalData"/> and written back.
/// </summary>
/// <remarks>
/// The discriminator and the field-name lists are plain strings, not enums: the set grows with TheHive releases, so a read value
/// round-trips through get, modify and update instead of becoming <c>Unknown</c>. The valid values come from
/// <c>ICaseReportTemplates.GetOptionsAsync</c>.
/// </remarks>
public sealed class CaseReportWidget
{
	/// <summary>The widget type; see <see cref="CaseReportWidgetKinds"/>. Required by the server.</summary>
	[JsonPropertyName("_kind")]
	public string? Kind { get; set; }

	/// <summary>The title displayed above the widget (all kinds).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The Mustache template rendered as the widget body (<c>Text</c>; required for that kind).</summary>
	[JsonPropertyName("template")]
	public string? Template { get; set; }

	/// <summary>The ID of the template attachment used as the image (<c>Image</c>; required for that kind).</summary>
	[JsonPropertyName("attachmentId")]
	public string? AttachmentId { get; set; }

	/// <summary>The fields shown as table columns (<c>AlertTable</c>, <c>ObservableTable</c>, <c>TaskTable</c>, <c>TTPTable</c>).</summary>
	[JsonPropertyName("columns")]
	public List<string>? Columns { get; set; }

	/// <summary>The fields shown for each list item (<c>AlertList</c>, <c>ObservableList</c>, <c>TaskList</c>, <c>TTPList</c>).</summary>
	[JsonPropertyName("fields")]
	public List<string>? Fields { get; set; }

	/// <summary>The task log fields shown under each task when <see cref="WithTaskLogs"/> is true (<c>TaskList</c>).</summary>
	[JsonPropertyName("logColumns")]
	public List<string>? LogColumns { get; set; }

	/// <summary>Whether task logs are included under each task (<c>TaskList</c>; the server default is true).</summary>
	[JsonPropertyName("withTaskLogs")]
	public bool? WithTaskLogs { get; set; }

	/// <summary>The filter applied to the widget's items, as a JSON object using the Query API <c>filter</c> syntax (list, table, <c>Comments</c> and <c>Pages</c> kinds).</summary>
	[JsonPropertyName("filter")]
	public JsonElement? Filter { get; set; }

	/// <summary>
	/// The sort order. Each entry is a JSON string (a field name prefixed with <c>+</c> or <c>-</c>) or an object of the Query API sort syntax
	/// (the spec allows several forms), so entries are raw JSON; build a string entry with <c>JsonSerializer.SerializeToElement("+_createdAt")</c>.
	/// </summary>
	[JsonPropertyName("sort")]
	public List<JsonElement>? Sort { get; set; }

	/// <summary>Whether the data of observables flagged as sensitive is masked (<c>ObservableTable</c>, <c>ObservableList</c>; required for those kinds).</summary>
	[JsonPropertyName("protectData")]
	public bool? ProtectData { get; set; }

	/// <summary>The maximum number of items in the widget; when omitted the global limit applies (list, table, <c>Comments</c> kinds).</summary>
	[JsonPropertyName("maxElements")]
	public int? MaxElements { get; set; }

	/// <summary>The event types included (<c>Timeline</c>).</summary>
	[JsonPropertyName("events")]
	public List<string>? Events { get; set; }

	/// <summary>Whether the description of custom events is included (<c>Timeline</c>; required for that kind).</summary>
	[JsonPropertyName("withCustomEventsDescription")]
	public bool? WithCustomEventsDescription { get; set; }

	/// <summary>Members the server sent that this class does not model; written back on update.</summary>
	[JsonExtensionData]
	public Dictionary<string, JsonElement>? AdditionalData { get; set; }
}
