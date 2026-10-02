using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of a set-case-access request (the spec's <c>InputManageCaseAccess</c>).</summary>
public sealed class CaseAccessRequest
{
	/// <summary>The new access mode. Switch to <see cref="AccessKind.OrganisationAccessKind"/> before moving between restricted and external access.</summary>
	[JsonPropertyName("access")]
	public required Access Access { get; set; }
}
