using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReports;

/// <summary>The output format of a generated or rendered case report.</summary>
public enum CaseReportFormat
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>An HTML document (<c>html</c>).</summary>
	[JsonStringEnumMemberName("html")]
	Html,

	/// <summary>A Markdown document (<c>markdown</c>).</summary>
	[JsonStringEnumMemberName("markdown")]
	Markdown,

	/// <summary>A Word document (<c>word</c>).</summary>
	[JsonStringEnumMemberName("word")]
	Word
}
