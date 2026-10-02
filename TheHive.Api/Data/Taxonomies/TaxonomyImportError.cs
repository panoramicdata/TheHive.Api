using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Taxonomies;

/// <summary>A file that failed to import (the spec's <c>Error</c>).</summary>
public sealed class TaxonomyImportError
{
	/// <summary>The name of the file that caused the error (for example <c>machinetag.json</c>).</summary>
	[JsonPropertyName("file")]
	public string File { get; set; } = string.Empty;

	/// <summary>A description of the error.</summary>
	[JsonPropertyName("error")]
	public string Error { get; set; } = string.Empty;

	/// <summary>Additional error details; the spec only says it is an object.</summary>
	[JsonPropertyName("details")]
	public JsonElement? Details { get; set; }

	/// <summary>The import status; always <c>Failure</c>.</summary>
	[JsonPropertyName("status")]
	public string Status { get; set; } = string.Empty;
}
