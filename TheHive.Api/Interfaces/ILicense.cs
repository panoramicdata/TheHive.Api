using System.Text.Json;
using Refit;
using TheHive.Api.Data.Licenses;

namespace TheHive.Api.Interfaces;

/// <summary>
/// License operations (the spec tag <c>License</c>). Except <see cref="GetCurrentAsync"/>, which any authenticated user may call, they require
/// <c>managePlatform</c>; send the <c>X-Organisation: admin</c> header (<see cref="TheHiveClientOptions.Organisation"/>) to target the admin organization.
/// License keys are secrets: they travel only in the body of <see cref="AddAsync"/>.
/// </summary>
public interface ILicense
{
	/// <summary>Lists all licenses stored in TheHive.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The licenses; empty when none is stored.</returns>
	[Get("api/v1/license")]
	Task<LicenseList> ListAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Adds a license and attempts to activate it. Obtain the challenge with <see cref="GetChallengeAsync"/> and submit it to the StrangeBee
	/// license portal to get the key.
	/// </summary>
	/// <param name="request">The license key. SECRET.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// <see langword="null"/> on success (HTTP 204). If the license was added but activation failed because it has expired or usage limits are
	/// exceeded, the server answers 200 with an error payload that the spec does not describe; it is returned as raw JSON (verify) and is not thrown.
	/// </returns>
	[Post("api/v1/license")]
	Task<JsonElement?> AddAsync([Body] LicenseAddRequest request, CancellationToken cancellationToken);

	/// <summary>Gets a stored license.</summary>
	/// <param name="licenseId">The license ID preceded by <c>~</c>, or the license file identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The license.</returns>
	[Get("api/v1/license/{licenseId}")]
	Task<License> GetAsync(string licenseId, CancellationToken cancellationToken);

	/// <summary>Activates a license that was previously added.</summary>
	/// <param name="licenseId">The license ID preceded by <c>~</c>, or the license file identifier.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/license/{licenseId}/activate")]
	Task ActivateAsync(string licenseId, CancellationToken cancellationToken);

	/// <summary>Gets the encrypted challenge token specific to this instance, to send to the StrangeBee license portal in exchange for a license key.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The raw <c>text/plain</c> challenge token.</returns>
	[Get("api/v1/license/challenge")]
	Task<string> GetChallengeAsync(CancellationToken cancellationToken);

	/// <summary>Gets the current license status: the valid license, or a fallback (with an error when validation failed).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The current license status.</returns>
	[Get("api/v1/license/current")]
	Task<LicenseCurrent> GetCurrentAsync(CancellationToken cancellationToken);
}
