using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>
/// The result of running function code (the spec's <c>OutputInvokeFunctionOk</c>). A function started in the background
/// (<c>sync=false</c> on an object invocation) answers with an empty object (the spec's <c>OutputInvokeFunctionBackground</c>),
/// which reads as the defaults here: <see cref="Result"/> is <see langword="null"/> and the other members are empty or zero.
/// </summary>
public sealed class FunctionInvocationResult
{
	/// <summary>The value returned by the <c>handle</c> function; free-form JSON.</summary>
	[JsonPropertyName("result")]
	public JsonElement? Result { get; set; }

	/// <summary>The time the function took, in milliseconds.</summary>
	[JsonPropertyName("durationMillis")]
	public long DurationMillis { get; set; }

	/// <summary>The content written to stdout.</summary>
	[JsonPropertyName("stdout")]
	public string Stdout { get; set; } = string.Empty;

	/// <summary>The content written to stderr.</summary>
	[JsonPropertyName("stderr")]
	public string Stderr { get; set; } = string.Empty;
}
