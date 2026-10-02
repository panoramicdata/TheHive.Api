using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Status;

/// <summary>The status of the built-in reference data imports (the spec's <c>Imports</c>).</summary>
public sealed class ImportsStatus
{
	/// <summary>The status of the built-in MITRE ATT&amp;CK catalog import: <c>Idle</c>, <c>Loading</c>, <c>Failed</c> or <c>Complete</c>. Kept as the string the server sent.</summary>
	[JsonPropertyName("mitreCatalog")]
	public string MitreCatalog { get; set; } = string.Empty;
}
