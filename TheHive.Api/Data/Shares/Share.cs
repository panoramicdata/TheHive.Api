using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Shares;

/// <summary>A sharing record of a case with one organization (the spec's <c>OutputShare</c>).</summary>
public sealed class Share
{
	/// <summary>The internal identifier of the share (for example <c>~1234567890</c>); pass it to <c>IShares.DeleteAsync</c> or <c>UpdateAsync</c>.</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Share</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the share.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the share, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the share was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the share was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The ID of the case this share belongs to (for example <c>~4123456789</c>).</summary>
	[JsonPropertyName("caseId")]
	public string CaseId { get; set; } = string.Empty;

	/// <summary>The name of the permission profile granted to the organization.</summary>
	[JsonPropertyName("profileName")]
	public string ProfileName { get; set; } = string.Empty;

	/// <summary>The name of the organization this share is for.</summary>
	[JsonPropertyName("organisationName")]
	public string OrganisationName { get; set; } = string.Empty;

	/// <summary>Whether this organization owns the case.</summary>
	[JsonPropertyName("owner")]
	public bool Owner { get; set; }

	/// <summary>The task-sharing rule for the organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule TaskRule { get; set; }

	/// <summary>The observable-sharing rule for the organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule ObservableRule { get; set; }
}
