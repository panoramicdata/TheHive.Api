using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cortex;

/// <summary>A responder available for one specific entity (the spec's <c>ListRespondersOutput</c>).</summary>
public sealed class CortexEntityResponder
{
	/// <summary>The ID of the responder once enabled within the organization.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The display name of the responder.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The version of the responder.</summary>
	[JsonPropertyName("version")]
	public string Version { get; set; } = string.Empty;

	/// <summary>A description of what the responder does.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The data types the responder supports.</summary>
	[JsonPropertyName("dataTypeList")]
	public List<string> DataTypeList { get; set; } = [];

	/// <summary>The names of the Cortex servers on which the responder is available.</summary>
	[JsonPropertyName("cortexIds")]
	public List<string> CortexIds { get; set; } = [];
}
