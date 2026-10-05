using Refit;
using TheHive.Api.Data.Permissions;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on the permissions that profiles can grant.</summary>
public interface IPermissions
{
	/// <summary>Lists every permission available in TheHive, with its label, licence consumption and scopes.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The permissions; use their <see cref="PermissionDescription.Name"/> in profile permission lists.</returns>
	[Get("api/v1/permission")]
	Task<List<PermissionDescription>> ListAsync(CancellationToken cancellationToken);
}
