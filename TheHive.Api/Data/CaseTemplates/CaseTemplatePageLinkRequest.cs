using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseTemplates;

/// <summary>The body of a link-page-templates request (the spec's <c>InputLinkPageTemplatesToCaseTemplate</c>).</summary>
public sealed class CaseTemplatePageLinkRequest
{
	/// <summary>The IDs (preceded by <c>~</c>) of the page templates to link; they replace every existing link, and an empty list removes all links. Omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("pageTemplateIds")]
	public List<string>? PageTemplateIds { get; set; }
}
