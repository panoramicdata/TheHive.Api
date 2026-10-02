using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.TaskLogs;

/// <summary>The body of an update-task-log request (the spec's <c>InputUpdateLog</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class TaskLogUpdateRequest
{
	/// <summary>The new content of the log (TheHive-flavored Markdown, at most 1048576 characters).</summary>
	[JsonPropertyName("message")]
	public string? Message { get; set; }

	/// <summary>The new date at which to pin the log in the case timeline; set to <see langword="null"/> to remove the pin.</summary>
	[JsonPropertyName("includeInTimeline")]
	public Optional<DateTimeOffset?> IncludeInTimeline { get; set; }
}
