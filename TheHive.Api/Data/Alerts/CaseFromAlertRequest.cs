using System.Text.Json.Serialization;
using TheHive.Api.Data.Cases;

namespace TheHive.Api.Data.Alerts;

/// <summary>
/// The optional body of a create-case-from-alert request (the spec's <c>InputCreateCaseFromAlert</c>). Unset properties are omitted
/// and the case inherits the value from the alert.
/// </summary>
public sealed class CaseFromAlertRequest : CaseRequestBase
{
	/// <summary>The title of the case (1 to 512 characters). When omitted, the alert title is used.</summary>
	[JsonPropertyName("title")]
	[JsonPropertyOrder(1)]
	public string? Title { get; set; }

	/// <summary>The details of the case (TheHive-flavored Markdown, at most 1048576 characters). When omitted, the alert description is used.</summary>
	[JsonPropertyName("description")]
	[JsonPropertyOrder(2)]
	public string? Description { get; set; }
}
