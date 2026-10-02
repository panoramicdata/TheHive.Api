using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>A JavaScript function (the spec's <c>OutputFunction</c>).</summary>
public sealed class Function
{
	/// <summary>The unique identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Function</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the function.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>When the function was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The login of the user who last updated the function.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the function was last updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The name of the function.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The execution mode: <c>Enabled</c>, <c>Disabled</c> or <c>DryRun</c> (a string in the spec's output, unlike the enum used in requests).</summary>
	[JsonPropertyName("mode")]
	public string Mode { get; set; } = string.Empty;

	/// <summary>The JavaScript source code.</summary>
	[JsonPropertyName("definition")]
	public string Definition { get; set; } = string.Empty;

	/// <summary>A description of the function.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The configuration object passed to the function at runtime. It is free-form and may hold secrets such as API keys:
	/// treat it as sensitive and do not log it.
	/// </summary>
	[JsonPropertyName("config")]
	public Dictionary<string, JsonElement> Config { get; set; } = [];

	/// <summary>When the function last ran successfully.</summary>
	[JsonPropertyName("lastSuccessDate")]
	public DateTimeOffset? LastSuccessDate { get; set; }

	/// <summary>Details of the last successful execution; free-form JSON.</summary>
	[JsonPropertyName("lastSuccessDetails")]
	public JsonElement? LastSuccessDetails { get; set; }

	/// <summary>When the function last failed.</summary>
	[JsonPropertyName("lastErrorDate")]
	public DateTimeOffset? LastErrorDate { get; set; }

	/// <summary>Details of the last failed execution; free-form JSON.</summary>
	[JsonPropertyName("lastErrorDetails")]
	public JsonElement? LastErrorDetails { get; set; }

	/// <summary>The function types, as strings (for example <c>feeder:alert</c> for an alert feeder function).</summary>
	[JsonPropertyName("types")]
	public List<string> Types { get; set; } = [];
}
