using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>The body of a create-function request (the spec's <c>InputFunction</c>). Unset properties are omitted.</summary>
public sealed class FunctionCreateRequest
{
	/// <summary>The name (1 to 128 characters); it must be unique within the organization.</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>The execution mode.</summary>
	[JsonPropertyName("mode")]
	public FunctionMode? Mode { get; set; }

	/// <summary>The JavaScript source code (up to 1048576 characters); it must define a <c>handle(input, context)</c> function.</summary>
	[JsonPropertyName("definition")]
	public required string Definition { get; set; }

	/// <summary>A description of the function.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>
	/// The configuration object passed to the function at runtime. It is free-form and may hold secrets such as API keys:
	/// they travel only in this request body.
	/// </summary>
	[JsonPropertyName("config")]
	public Dictionary<string, object?>? Config { get; set; }

	/// <summary>The function types, which determine how and when the function can be invoked.</summary>
	[JsonPropertyName("types")]
	public List<FunctionType>? Types { get; set; }
}
