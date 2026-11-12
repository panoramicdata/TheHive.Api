using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.CaseReports;

/// <summary>A report file attached to a case (the spec's <c>OutputCaseReport</c>).</summary>
public sealed class CaseReport : TheHiveEntity
{
	/// <summary>The MIME type of the report file.</summary>
	[JsonPropertyName("contentType")]
	public string ContentType { get; set; } = string.Empty;

	/// <summary>The size of the report file in bytes.</summary>
	[JsonPropertyName("size")]
	public long Size { get; set; }

	/// <summary>The name of the report file.</summary>
	[JsonPropertyName("filename")]
	public string Filename { get; set; } = string.Empty;
}
