using Refit;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Platform administration operations. Requires <c>managePlatform</c>; send the <c>X-Organisation: admin</c> header
/// (<see cref="TheHiveClientOptions.Organisation"/>) to target the admin organization. The other administration areas have their own
/// interfaces: <c>ILicense</c>, <c>IBranding</c>, <c>IAlertStatuses</c>, <c>ICaseStatuses</c> and <c>IStatus</c>.
/// </summary>
public interface IAdmin
{
	/// <summary>Sets the log level of a logger at runtime, without restarting TheHive. The change takes effect immediately and is lost when TheHive restarts.</summary>
	/// <param name="packageName">The logger name.</param>
	/// <param name="level">The level: one of the <see cref="Data.Admin.LogLevels"/> constants (<c>ALL</c>, <c>TRACE</c>, <c>DEBUG</c>, <c>INFO</c>, <c>WARN</c>, <c>ERROR</c>, <c>OFF</c>).</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/admin/log/set/{packageName}/{level}")]
	Task SetLogLevelAsync(string packageName, string level, CancellationToken cancellationToken = default);
}
