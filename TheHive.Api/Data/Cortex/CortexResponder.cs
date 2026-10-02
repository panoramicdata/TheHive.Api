using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>A responder that can run on an entity type (the spec's second <c>OutputWorker</c> schema, listing responders by entity type).</summary>
public sealed class CortexResponder
{
	/// <summary>The ID of the responder once enabled within the organization.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The display name of the responder.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>A description of what the responder does.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;
}
