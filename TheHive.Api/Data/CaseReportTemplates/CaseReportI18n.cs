using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReportTemplates;

/// <summary>The language settings of a case report template definition (the spec's <c>I18n</c>).</summary>
public sealed class CaseReportI18n
{
	/// <summary>The BCP 47 language tag used to format dates and labels in the report (server default <c>en</c>).</summary>
	[JsonPropertyName("lang")]
	public string? Lang { get; set; }
}
