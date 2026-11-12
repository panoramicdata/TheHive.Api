using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Shares;

/// <summary>A sharing record of a case with one organization (the spec's <c>OutputShare</c>).</summary>
public sealed class Share : TheHiveEntity
{
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
