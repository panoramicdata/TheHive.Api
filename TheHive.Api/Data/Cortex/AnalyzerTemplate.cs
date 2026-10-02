using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>An analyzer report template (the spec's <c>OutputAnalyzerTemplate</c>).</summary>
public sealed class AnalyzerTemplate
{
	/// <summary>The internal identifier (preceded by <c>~</c>).</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The name of the analyzer the template is linked to.</summary>
	[JsonPropertyName("analyzerId")]
	public string AnalyzerId { get; set; } = string.Empty;

	/// <summary>The HTML content of the template.</summary>
	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;
}
