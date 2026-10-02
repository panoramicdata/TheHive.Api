using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Organisations;

/// <summary>An organization (the spec's <c>OutputOrganisation</c>).</summary>
public sealed class Organisation
{
	/// <summary>The internal identifier (for example <c>~128458762</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Organisation</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the organization.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the organization, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the organization was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the organization was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The unique name, which can be used instead of <see cref="Id"/>.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The task-sharing rule applied to new case shares with this organization.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule TaskRule { get; set; }

	/// <summary>The observable-sharing rule applied to new case shares with this organization.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule ObservableRule { get; set; }

	/// <summary>The sharing links configured for this organization.</summary>
	[JsonPropertyName("links")]
	public List<OrganisationLink> Links { get; set; } = [];

	/// <summary>The Base64-encoded avatar image, if the organization has one.</summary>
	[JsonPropertyName("avatar")]
	public string? Avatar { get; set; }

	/// <summary>Whether the organization is locked; members of a locked organization cannot log in.</summary>
	[JsonPropertyName("locked")]
	public bool Locked { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
