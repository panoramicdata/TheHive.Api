using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of an add- or remove-case-link request (the spec's <c>InputCaseLink</c>).</summary>
public sealed class CaseLinkRequest
{
	/// <summary>The link type (1 to 64 characters); a new name creates a new type. See <c>ICases.GetLinkTypesAsync</c>.</summary>
	[JsonPropertyName("type")]
	public required string Type { get; set; }

	/// <summary>The ID of the case to link to (for example <c>~72637286</c>).</summary>
	[JsonPropertyName("caseId")]
	public required string CaseId { get; set; }
}
