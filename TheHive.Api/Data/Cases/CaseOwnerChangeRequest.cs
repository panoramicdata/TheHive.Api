using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of a change-case-owner request (the spec's <c>InputChangeCaseOwnership</c>). Unset properties are omitted.</summary>
public sealed class CaseOwnerChangeRequest
{
	/// <summary>The name or ID of the organization to make the owner.</summary>
	[JsonPropertyName("organisation")]
	public required string Organisation { get; set; }

	/// <summary>The sharing profile the current owner keeps; if omitted, the current owner loses all access.</summary>
	[JsonPropertyName("keepProfile")]
	public string? KeepProfile { get; set; }

	/// <summary>The task-sharing rule for the new owner.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The observable-sharing rule for the new owner.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }
}
