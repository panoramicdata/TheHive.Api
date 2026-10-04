using System.Text.Json;
using Refit;
using TheHive.Api.Data.UserConfig;

namespace TheHive.Api.Interfaces;

/// <summary>
/// The configuration items of the calling user (the spec tag <c>Config</c>), such as <c>dashboards</c>, <c>list-views-cases</c>,
/// <c>notification.items</c>, <c>organisation</c> and <c>profile</c>. Each item holds any JSON value.
/// </summary>
public interface IConfig
{
	/// <summary>Lists the configuration of the calling user, merging saved overrides with the TheHive defaults.</summary>
	/// <param name="query">The key of a single configuration item (<see cref="UserConfigQuery.Path"/>); pass <c>new()</c> to return the full configuration.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The configuration as one JSON object (the spec leaves its schema open, so it is raw JSON).</returns>
	[Get("api/v1/config/user")]
	Task<JsonElement> ListAsync([Query] UserConfigQuery query, CancellationToken cancellationToken);

	/// <summary>Gets one configuration item.</summary>
	/// <param name="path">The key of the configuration item.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The item with its default and current values.</returns>
	[Get("api/v1/config/user/{path}")]
	Task<UserConfigItem> GetAsync(string path, CancellationToken cancellationToken);

	/// <summary>Creates or replaces one configuration item.</summary>
	/// <param name="path">The key of the configuration item.</param>
	/// <param name="request">The new value.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The item with its default and new current values.</returns>
	[Put("api/v1/config/user/{path}")]
	Task<UserConfigItem> SetAsync(string path, [Body] UserConfigSetRequest request, CancellationToken cancellationToken);
}
