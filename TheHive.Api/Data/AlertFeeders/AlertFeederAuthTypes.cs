namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The known values of <see cref="AlertFeederAuth.Type"/>, the <c>type</c> discriminator of the spec's authentication schemas.</summary>
public static class AlertFeederAuthTypes
{
	/// <summary>No authentication.</summary>
	public const string None = "none";

	/// <summary>HTTP Basic authentication (<see cref="AlertFeederAuth.Username"/> and <see cref="AlertFeederAuth.Password"/>).</summary>
	public const string Basic = "basic";

	/// <summary>A bearer token (<see cref="AlertFeederAuth.Key"/>).</summary>
	public const string Bearer = "bearer";

	/// <summary>An API key (<see cref="AlertFeederAuth.Key"/> and optional <see cref="AlertFeederAuth.Prefix"/>).</summary>
	public const string Key = "key";

	/// <summary>OAuth 2.0 client credentials.</summary>
	public const string OAuth2 = "oauth2";
}
