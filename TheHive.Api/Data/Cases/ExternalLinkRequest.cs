using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of an add- or remove-external-link request (the spec's <c>InputExternalLink</c>).</summary>
public sealed class ExternalLinkRequest
{
	/// <summary>The link type (1 to 64 characters); a new name creates a new type. See <c>ICases.GetLinkTypesAsync</c>.</summary>
	[JsonPropertyName("type")]
	public required string Type { get; set; }

	/// <summary>The URL of the external resource.</summary>
	[JsonPropertyName("url")]
	public required string Url { get; set; }
}
