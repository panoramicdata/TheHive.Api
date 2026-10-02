using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Patterns;

/// <summary>
/// The result of a MITRE ATT&amp;CK import. The spec types the body as an untyped object and shows <c>success</c> (the number of techniques imported,
/// answered with 201) and, for a partial import (207, a success status that does not throw), also <c>errors</c>. One type models both, with
/// <see cref="Errors"/> empty for the 201 form; (verify) the member names come from the spec's examples only.
/// </summary>
public sealed class PatternImportResult
{
	/// <summary>The number of techniques imported.</summary>
	[JsonPropertyName("success")]
	public int Success { get; set; }

	/// <summary>The techniques that were skipped, with the reason; empty when everything was imported.</summary>
	[JsonPropertyName("errors")]
	public List<string> Errors { get; set; } = [];
}
