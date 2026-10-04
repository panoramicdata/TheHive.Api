using Refit;
using TheHive.Api.Data.Profiles;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on permission profiles. Writes require <c>manageProfile</c> and a Gold or Platinum licence; send the
/// <c>X-Organisation: admin</c> header to target the admin organization. To list profiles use the query API with <c>listProfile</c>.
/// </summary>
public interface IProfiles
{
	/// <summary>Creates a custom permission profile.</summary>
	/// <param name="request">The profile to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created profile.</returns>
	[Post("api/v1/profile")]
	Task<Profile> CreateAsync([Body] ProfileCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a custom profile; it fails while the profile is assigned to a user, and the predefined profiles cannot be deleted.</summary>
	/// <param name="profileId">The profile ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/profile/{profileId}")]
	Task DeleteAsync(string profileId, CancellationToken cancellationToken);

	/// <summary>Gets a profile.</summary>
	/// <param name="profileId">The profile ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The profile.</returns>
	[Get("api/v1/profile/{profileId}")]
	Task<Profile> GetAsync(string profileId, CancellationToken cancellationToken);

	/// <summary>Updates a custom profile; the predefined profiles cannot be modified, except Analyst.</summary>
	/// <param name="profileId">The profile ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/profile/{profileId}")]
	Task UpdateAsync(string profileId, [Body] ProfileUpdateRequest request, CancellationToken cancellationToken);
}
