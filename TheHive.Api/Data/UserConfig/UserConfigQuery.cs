using Refit;

namespace TheHive.Api.Data.UserConfig;

/// <summary>
/// The optional query-string parameter of <c>IConfig.ListAsync</c>. Pass an empty instance (<c>new()</c>) to return the full configuration.
/// </summary>
public sealed class UserConfigQuery
{
	/// <summary>The key of a single configuration item, sent as <c>path</c>; left out when <see langword="null"/>, which returns the full configuration.</summary>
	[AliasAs("path")]
	public string? Path { get; set; }
}