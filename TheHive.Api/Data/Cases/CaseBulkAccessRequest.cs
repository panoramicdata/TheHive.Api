using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>The body of a bulk set-case-access request (the spec's <c>InputManageCaseAccessWithIds</c>). All cases must have the same initial access.</summary>
public sealed class CaseBulkAccessRequest
{
	/// <summary>The cases to update: IDs preceded by <c>~</c>, or case numbers.</summary>
	[JsonPropertyName("ids")]
	public required List<string> Ids { get; set; }

	/// <summary>The new access mode. Switch to <see cref="AccessKind.OrganisationAccessKind"/> before moving between restricted and external access.</summary>
	[JsonPropertyName("access")]
	public required Access Access { get; set; }
}
