using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CaseReports;

/// <summary>A report file attached to a case (the spec's <c>OutputCaseReport</c>).</summary>
public sealed class CaseReport
{
	/// <summary>The internal identifier (for example <c>~84512</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>CaseReport</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the report.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the report, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the report was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the report was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

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
