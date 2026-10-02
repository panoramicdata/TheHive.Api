using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Shares;

/// <summary>
/// The per-organisation share entry nested in create requests, such as <c>CaseCreateRequest.SharingParameters</c>
/// (the spec's <c>InputShare</c>). Unset properties are omitted.
/// </summary>
public sealed class ShareSettings
{
	/// <summary>The name or ID of a linked organization.</summary>
	[JsonPropertyName("organisation")]
	public required string Organisation { get; set; }

	/// <summary>Whether the organization has active sharing access. The server default is <see langword="true"/>.</summary>
	[JsonPropertyName("share")]
	public bool? Share { get; set; }

	/// <summary>The profile defining the organization members' permissions. The server default is <c>analyst</c>.</summary>
	[JsonPropertyName("profile")]
	public string? Profile { get; set; }

	/// <summary>The task-sharing rule for this organization. The server default is <see cref="SharingRule.Manual"/>.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The observable-sharing rule for this organization. The server default is <see cref="SharingRule.Manual"/>.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }
}
