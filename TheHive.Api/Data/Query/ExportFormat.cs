using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Query;

/// <summary>The file format of a query export.</summary>
public enum ExportFormat
{
	/// <summary>A value this client does not recognise; do not send it.</summary>
	Unknown = 0,

	/// <summary>CSV: a header row of field paths, then one row per entity (<c>text/csv</c>).</summary>
	[JsonStringEnumMemberName("csv")]
	Csv,

	/// <summary>One JSON array with one object per entity (<c>application/json</c>).</summary>
	[JsonStringEnumMemberName("json")]
	Json,

	/// <summary>Observables only: one value (or <c>filename|sha256</c>) per line (<c>text/plain</c>).</summary>
	[JsonStringEnumMemberName("txt")]
	Txt,

	/// <summary>Observables only: one MISP-style line per observable (<c>text/plain</c>).</summary>
	[JsonStringEnumMemberName("misp")]
	Misp
}
