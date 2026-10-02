using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>The outcome of deduplicating a case's observables (the spec's <c>OutputMergeCases</c>).</summary>
public sealed class ObservableDeduplicationResult
{
	/// <summary>The number of observables with no duplicates, left unchanged.</summary>
	[JsonPropertyName("untouched")]
	public int Untouched { get; set; }

	/// <summary>The number of distinct observables updated with metadata combined from their duplicates.</summary>
	[JsonPropertyName("updated")]
	public int Updated { get; set; }

	/// <summary>The number of duplicate observables removed.</summary>
	[JsonPropertyName("deleted")]
	public int Deleted { get; set; }
}
