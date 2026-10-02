using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>The body of a test-function request (the spec's <c>InputTestFunction</c>): code to run without saving it.</summary>
public sealed class FunctionTestRequest
{
	/// <summary>A name used for identification only (1 to 128 characters).</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The JavaScript source code to test (up to 1048576 characters); it must define a <c>handle(input, context)</c> function.</summary>
	[JsonPropertyName("definition")]
	public required string Definition { get; set; }

	/// <summary>The configuration object passed to the function at runtime. It may hold secrets: they travel only in this request body.</summary>
	[JsonPropertyName("config")]
	public Dictionary<string, object?>? Config { get; set; }

	/// <summary>The input passed to the <c>handle</c> function as its first argument; any JSON-serializable value.</summary>
	[JsonPropertyName("input")]
	public object? Input { get; set; }
}
