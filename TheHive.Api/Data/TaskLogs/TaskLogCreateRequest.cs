using System.Text.Json.Serialization;

namespace TheHive.Api.Data.TaskLogs;

/// <summary>The body of a create-task-log request (the spec's <c>InputCreateLog</c>, JSON form). Unset properties are omitted.</summary>
/// <remarks>The spec also accepts a multipart form with files; only the JSON form is modelled. Add files afterwards with <c>ITaskLogs.AddAttachmentsAsync</c>.</remarks>
public sealed class TaskLogCreateRequest
{
	/// <summary>The content of the log (TheHive-flavored Markdown, at most 1048576 characters). Do not include observables.</summary>
	[JsonPropertyName("message")]
	public required string Message { get; set; }

	/// <summary>The date and time of the log entry; the server uses the current time when omitted.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset? StartDate { get; set; }

	/// <summary>When set, pins the log to the case timeline at this date.</summary>
	[JsonPropertyName("includeInTimeline")]
	public DateTimeOffset? IncludeInTimeline { get; set; }
}
