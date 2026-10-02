using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Shares;

namespace TheHive.Api.Data.Cases;

/// <summary>The settings sent as the <c>_json</c> part of an import-case request (the spec's <c>InputImportCase</c>). Unset properties are omitted.</summary>
public sealed class CaseImportRequest
{
	/// <summary>The password set when the case was exported (1 to 512 characters).</summary>
	[JsonPropertyName("password")]
	public required string Password { get; set; }

	/// <summary>Per-organization sharing settings for the imported case.</summary>
	[JsonPropertyName("sharingParameters")]
	public List<ShareSettings>? SharingParameters { get; set; }

	/// <summary>The task-sharing rule for the owner organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The observable-sharing rule for the owner organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }
}
