using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Alerts;

/// <summary>The body of an update-alert request (the spec's <c>InputUpdateAlert</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
/// <remarks><see cref="AlertBulkUpdateRequest"/> derives from this class; passing a bulk request to <c>IAlerts.UpdateAsync</c> would also send its <c>ids</c>.</remarks>
public class AlertUpdateRequest
{
	/// <summary>The new alert type (1 to 32 characters).</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	/// <summary>The new source system (1 to 32 characters).</summary>
	[JsonPropertyName("source")]
	public string? Source { get; set; }

	/// <summary>The new source reference (1 to 128 characters).</summary>
	[JsonPropertyName("sourceRef")]
	public string? SourceRef { get; set; }

	/// <summary>The new external URL in the source system; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("externalLink")]
	public Optional<string?> ExternalLink { get; set; }

	/// <summary>The new title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new description (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>The new date of the event that triggered the alert.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset? Date { get; set; }

	/// <summary>When the alert was last synchronised with its source system.</summary>
	[JsonPropertyName("lastSyncDate")]
	public DateTimeOffset? LastSyncDate { get; set; }

	/// <summary>Replaces all current tags with this set.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>The new Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The new Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>Whether to follow the alert for change notifications.</summary>
	[JsonPropertyName("follow")]
	public bool? Follow { get; set; }

	/// <summary>The custom field values; in this array form, custom fields not listed are deleted.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The name of an existing alert status; a status linked to the <c>Closed</c> stage closes the alert.</summary>
	[JsonPropertyName("status")]
	public string? Status { get; set; }

	/// <summary>The new triage summary; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("summary")]
	public Optional<string?> Summary { get; set; }

	/// <summary>The login of the user to assign; set to <see langword="null"/> to unassign.</summary>
	[JsonPropertyName("assignee")]
	public Optional<string?> Assignee { get; set; }

	/// <summary>Tags to add to the current set.</summary>
	[JsonPropertyName("addTags")]
	public List<string>? AddTags { get; set; }

	/// <summary>Tags to remove from the current set; absent tags are ignored.</summary>
	[JsonPropertyName("removeTags")]
	public List<string>? RemoveTags { get; set; }
}
