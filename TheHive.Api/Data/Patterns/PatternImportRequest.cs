using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Patterns;

/// <summary>
/// The settings of a MITRE ATT&amp;CK import (the spec's <c>InputPatternImportMitre</c>): the JSON body of the URL import, or the <c>_json</c> part
/// of the file import. Unset properties are omitted.
/// </summary>
public sealed class PatternImportRequest
{
	/// <summary>The URL of the MITRE ATT&amp;CK JSON file to import (1 to 512 characters). Use this or an uploaded file, not both; leave it unset for a file import.</summary>
	[JsonPropertyName("url")]
	public string? Url { get; set; }

	/// <summary>The name or ID of the catalog to import into; TheHive creates a catalog with that name when none exists (1 to 128 characters).</summary>
	[JsonPropertyName("catalog")]
	public required string Catalog { get; set; }

	/// <summary>The variant identifier set on the catalog after import; defaults to today's date (<c>YYYY-MM-DD</c>) when unset.</summary>
	[JsonPropertyName("variant")]
	public string? Variant { get; set; }
}
