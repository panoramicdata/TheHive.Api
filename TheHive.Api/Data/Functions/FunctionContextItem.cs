using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>A field or method available on the <c>context</c> object passed to function code (the spec's <c>OutputContextDocumentationItem</c>).</summary>
public sealed class FunctionContextItem
{
	/// <summary>The name of the context field or method.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>Whether this item is a field or a method.</summary>
	[JsonPropertyName("kind")]
	public FunctionContextItemKind Kind { get; set; }

	/// <summary>The parameter names the method accepts; present only for a method.</summary>
	[JsonPropertyName("args")]
	public List<string>? Args { get; set; }
}
