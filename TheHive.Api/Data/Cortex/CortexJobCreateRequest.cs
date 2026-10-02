using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>The body of a run-analyzer request (the spec's <c>InputJob</c>).</summary>
public sealed class CortexJobCreateRequest
{
	/// <summary>The ID of the analyzer to run.</summary>
	[JsonPropertyName("analyzerId")]
	public required string AnalyzerId { get; set; }

	/// <summary>The name of the Cortex server to run the analyzer on.</summary>
	[JsonPropertyName("cortexId")]
	public required string CortexId { get; set; }

	/// <summary>The ID of the observable to analyze (preceded by <c>~</c>).</summary>
	[JsonPropertyName("artifactId")]
	public required string ArtifactId { get; set; }

	/// <summary>Extra parameters passed to the analyzer; the keys are defined by the analyzer's own configuration on Cortex.</summary>
	[JsonPropertyName("parameters")]
	public Dictionary<string, object?>? Parameters { get; set; }
}
