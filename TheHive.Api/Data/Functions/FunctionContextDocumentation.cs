using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>The documentation of the <c>context</c> object available to function code (the spec's <c>OutputContextDocumentation</c>).</summary>
public sealed class FunctionContextDocumentation
{
	/// <summary>The context fields and methods available in the function execution environment.</summary>
	[JsonPropertyName("items")]
	public List<FunctionContextItem> Items { get; set; } = [];
}
