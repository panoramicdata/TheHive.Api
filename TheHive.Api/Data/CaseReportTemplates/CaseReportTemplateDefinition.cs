using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The layout of a case report template (the spec's <c>CaseReportTemplateDefinition</c>), used in template requests, template responses and the inline-render request.</summary>
public sealed class CaseReportTemplateDefinition
{
	/// <summary>The ordered widgets that define the report body.</summary>
	[JsonPropertyName("widgets")]
	public List<CaseReportWidget>? Widgets { get; set; }

	/// <summary>The header template rendered at the top of each page.</summary>
	[JsonPropertyName("header")]
	public CaseReportHeader? Header { get; set; }

	/// <summary>The footer template rendered at the bottom of each page.</summary>
	[JsonPropertyName("footer")]
	public CaseReportFooter? Footer { get; set; }

	/// <summary>The date format pattern for date fields (server default <c>yyyy-MM-dd</c>).</summary>
	[JsonPropertyName("dateFormat")]
	public string? DateFormat { get; set; }

	/// <summary>The date and time format pattern for datetime fields (server default <c>yyyy-MM-dd HH:mm</c>).</summary>
	[JsonPropertyName("dateTimeFormat")]
	public string? DateTimeFormat { get; set; }

	/// <summary>The language settings; the spec requires this object, so it is always sent (empty by default, which means <c>en</c>).</summary>
	[JsonPropertyName("i18n")]
	public CaseReportI18n I18n { get; set; } = new();
}
