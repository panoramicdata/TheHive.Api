using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>The body of an update-function request (the spec's <c>InputUpdateFunction</c>). Unset properties are omitted; the spec defines no clearable fields.</summary>
public sealed class FunctionUpdateRequest
{
	/// <summary>The new execution mode.</summary>
	[JsonPropertyName("mode")]
	public FunctionMode? Mode { get; set; }

	/// <summary>The new JavaScript source code (up to 1048576 characters).</summary>
	[JsonPropertyName("definition")]
	public string? Definition { get; set; }

	/// <summary>The new description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new configuration object. It is free-form and may hold secrets such as API keys: they travel only in this request body.</summary>
	[JsonPropertyName("config")]
	public Dictionary<string, object?>? Config { get; set; }

	/// <summary>The new function types.</summary>
	[JsonPropertyName("types")]
	public List<FunctionType>? Types { get; set; }
}
