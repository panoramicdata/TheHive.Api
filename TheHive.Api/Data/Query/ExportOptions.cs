using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Query;

/// <summary>The <c>options</c> of a query export, sent as JSON text in the <c>options</c> query-string parameter.</summary>
public sealed class ExportOptions
{
	/// <summary>The file format.</summary>
	[JsonPropertyName("format")]
	public required ExportFormat Format { get; init; }

	/// <summary>The file name without extension; the server defaults to <c>&lt;YYYY-MM-DD&gt;_export</c>.</summary>
	[JsonPropertyName("fileName")]
	public string? FileName { get; init; }

	/// <summary>The entity model the query returns (mandatory since TheHive 5.3).</summary>
	[JsonPropertyName("model")]
	public required ExportModel Model { get; init; }

	/// <summary>Whether to protect the observable <c>data</c> field; the server defaults to <see langword="true"/>.</summary>
	[JsonPropertyName("protectData")]
	public bool? ProtectData { get; init; }

	/// <summary>
	/// CSV only: the field paths to export instead of the default selection (list them with
	/// <see cref="Interfaces.IQuery.GetExportFieldsAsync"/>). <c>customFields</c> expands to all custom fields and
	/// <c>customFields.&lt;name&gt;</c> selects one.
	/// </summary>
	[JsonPropertyName("fields")]
	public List<string>? Fields { get; init; }

	/// <summary>CSV only: the field delimiter; the server defaults to a comma.</summary>
	[JsonPropertyName("delimiter")]
	public char? Delimiter { get; init; }

	/// <summary>CSV only: the quote character; the server defaults to a double quote.</summary>
	[JsonPropertyName("quoteChar")]
	public char? QuoteChar { get; init; }

	/// <summary>CSV only: the escape character; the server defaults to a backslash.</summary>
	[JsonPropertyName("escapeChar")]
	public char? EscapeChar { get; init; }
}
