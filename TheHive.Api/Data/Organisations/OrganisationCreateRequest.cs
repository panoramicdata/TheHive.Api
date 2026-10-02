using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Organisations;

/// <summary>
/// The body of a create-organization request (the spec's <c>InputCreateOrganisation</c>). Unset properties are omitted.
/// Only the JSON form is modelled; the spec's multipart form, which adds an avatar file, is not: set the avatar afterwards with
/// <see cref="OrganisationUpdateRequest.Avatar"/>.
/// </summary>
public sealed class OrganisationCreateRequest
{
	/// <summary>The name (1 to 64 characters).</summary>
	[JsonPropertyName("name")]
	public required string Name { get; set; }

	/// <summary>A short description.</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The default task-sharing rule for new case shares with this organization. The server default is <see cref="SharingRule.Manual"/>.</summary>
	[JsonPropertyName("taskRule")]
	public SharingRule? TaskRule { get; set; }

	/// <summary>The default observable-sharing rule for new case shares with this organization. The server default is <see cref="SharingRule.Manual"/>.</summary>
	[JsonPropertyName("observableRule")]
	public SharingRule? ObservableRule { get; set; }

	/// <summary>Whether to lock the organization immediately; members of a locked organization cannot access it.</summary>
	[JsonPropertyName("locked")]
	public bool? Locked { get; set; }
}
