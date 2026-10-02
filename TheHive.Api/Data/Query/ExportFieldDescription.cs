using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Query;

/// <summary>One exportable field of a model (the spec's <c>ExportFieldDescription</c>).</summary>
public sealed class ExportFieldDescription
{
	/// <summary>The field path to use in <see cref="ExportOptions.Fields"/>; nested fields use dots, for example <c>attachment.name</c>.</summary>
	[JsonPropertyName("fieldPath")]
	public string FieldPath { get; set; } = string.Empty;

	/// <summary>
	/// The logical type: <c>string</c>, <c>string[]</c>, <c>number</c>, <c>boolean</c>, <c>date</c>, <c>user</c>, <c>customFields</c>,
	/// <c>organisationLink[]</c> or <c>organisationProfile[]</c>.
	/// </summary>
	[JsonPropertyName("fieldType")]
	public string FieldType { get; set; } = string.Empty;

	/// <summary>Whether the field is exported when <see cref="ExportOptions.Fields"/> is not set in a CSV export.</summary>
	[JsonPropertyName("selectedByDefault")]
	public bool SelectedByDefault { get; set; }
}
