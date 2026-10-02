using Refit;
using TheHive.Api.Data.Status;

namespace TheHive.Api.Interfaces;

/// <summary>Platform status operations (the spec tag <c>Status</c>).</summary>
public interface IStatus
{
	/// <summary>Gets the status of the instance: version, license validity, connector status and enabled feature flags.</summary>
	/// <param name="verbose">Whether to also include the cluster state and the database schema version of each module; sent as <c>true</c> or <c>false</c>, and omitted when <see langword="null"/> (the server default is <c>false</c>).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The platform status.</returns>
	[Get("api/v1/status")]
	Task<PlatformStatus> GetAsync([Query] bool? verbose = null, CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets the platform information that needs no authentication: whether single sign-on is on, the SSO providers, the version and the status of
	/// the built-in MITRE ATT&amp;CK catalog import. The client still sends its API key; the endpoint ignores it.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The public status.</returns>
	[Get("api/v1/status/public")]
	Task<PublicStatus> GetPublicAsync(CancellationToken cancellationToken = default);
}
