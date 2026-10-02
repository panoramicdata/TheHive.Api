using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>The body of an update-analyzer-template request (the spec's <c>InputAnalyzerTemplateUpdate</c>). The spec defines no clearable fields.</summary>
public sealed class AnalyzerTemplateUpdateRequest
{
	/// <summary>The new HTML content of the template; omitted when <see langword="null"/>.</summary>
	[JsonPropertyName("content")]
	public string? Content { get; set; }
}
