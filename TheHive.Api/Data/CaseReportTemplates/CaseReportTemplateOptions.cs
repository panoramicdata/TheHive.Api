using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The widget types and field names a template definition may use (the spec's <c>OutputCaseReportTemplateObjects</c>). All values are strings so values added by newer servers are kept.</summary>
public sealed class CaseReportTemplateOptions
{
	/// <summary>The available widget types (see <see cref="CaseReportWidgetKinds"/>).</summary>
	[JsonPropertyName("widgets")]
	public List<string> Widgets { get; set; } = [];

	/// <summary>The alert fields available to alert widgets.</summary>
	[JsonPropertyName("alertFields")]
	public List<string> AlertFields { get; set; } = [];

	/// <summary>The observable fields available to observable widgets.</summary>
	[JsonPropertyName("observableFields")]
	public List<string> ObservableFields { get; set; } = [];

	/// <summary>The task fields available to task widgets.</summary>
	[JsonPropertyName("taskFields")]
	public List<string> TaskFields { get; set; } = [];

	/// <summary>The TTP fields available to TTP widgets.</summary>
	[JsonPropertyName("ttpFields")]
	public List<string> TtpFields { get; set; } = [];

	/// <summary>The task log fields available to task list widgets.</summary>
	[JsonPropertyName("logFields")]
	public List<string> LogFields { get; set; } = [];

	/// <summary>The event types available to timeline widgets.</summary>
	[JsonPropertyName("timelineEvents")]
	public List<string> TimelineEvents { get; set; } = [];
}
