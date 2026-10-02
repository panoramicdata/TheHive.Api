using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Procedures;

namespace TheHive.Api.Data.Cases;

/// <summary>The result of importing a case archive (the spec's <c>OutputImportCase</c>).</summary>
public sealed class CaseImportResult
{
	/// <summary>The imported case.</summary>
	[JsonPropertyName("case")]
	public Case Case { get; set; } = new();

	/// <summary>The observables restored from the archive.</summary>
	[JsonPropertyName("observables")]
	public List<Observable> Observables { get; set; } = [];

	/// <summary>The procedures (TTPs) restored from the archive.</summary>
	[JsonPropertyName("procedures")]
	public List<Procedure> Procedures { get; set; } = [];

	/// <summary>The errors encountered during the import, if any; the spec does not describe their shape.</summary>
	[JsonPropertyName("errors")]
	public List<JsonElement> Errors { get; set; } = [];
}
