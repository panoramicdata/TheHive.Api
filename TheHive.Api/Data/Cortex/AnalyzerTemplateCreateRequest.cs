using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>The body of a create-analyzer-template request (the spec's <c>InputAnalyzerTemplate</c>).</summary>
public sealed class AnalyzerTemplateCreateRequest
{
	/// <summary>The name of the analyzer the template applies to; it must match the <c>name</c> of an analyzer available from a connected Cortex server.</summary>
	[JsonPropertyName("analyzerId")]
	public required string AnalyzerId { get; set; }

	/// <summary>The HTML content of the template.</summary>
	[JsonPropertyName("content")]
	public required string Content { get; set; }
}
