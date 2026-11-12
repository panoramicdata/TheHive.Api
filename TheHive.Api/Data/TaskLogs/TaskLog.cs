using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.TaskLogs;

/// <summary>A log entry on a task (the spec's <c>OutputLog</c>).</summary>
public sealed class TaskLog : TheHiveEntity
{
	/// <summary>The content of the log (TheHive-flavored Markdown).</summary>
	[JsonPropertyName("message")]
	public string Message { get; set; } = string.Empty;

	/// <summary>The date and time of the log entry.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset Date { get; set; }

	/// <summary>The files attached to the log.</summary>
	[JsonPropertyName("attachments")]
	public List<Attachment> Attachments { get; set; } = [];

	/// <summary>The name of the organization that owns the log.</summary>
	[JsonPropertyName("owner")]
	public string Owner { get; set; } = string.Empty;

	/// <summary>The date at which the log is pinned in the case timeline, if it is pinned.</summary>
	[JsonPropertyName("includeInTimeline")]
	public DateTimeOffset? IncludeInTimeline { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
