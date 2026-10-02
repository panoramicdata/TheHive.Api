using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>An analyzer available to the organization (the spec's <c>AnalyzerOutput</c> and <c>OutputWorker</c>, which are identical).</summary>
public sealed class CortexAnalyzer
{
	/// <summary>The ID of the analyzer once enabled within the organization.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The display name of the analyzer.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The version of the analyzer.</summary>
	[JsonPropertyName("version")]
	public string Version { get; set; } = string.Empty;

	/// <summary>A description of what the analyzer does.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The observable data types the analyzer supports.</summary>
	[JsonPropertyName("dataTypeList")]
	public List<string> DataTypeList { get; set; } = [];

	/// <summary>The names of the Cortex servers on which the analyzer is available.</summary>
	[JsonPropertyName("cortexIds")]
	public List<string> CortexIds { get; set; } = [];
}
