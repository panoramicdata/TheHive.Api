using Refit;

namespace TheHive.Api.Data.Status;

/// <summary>
/// The optional query-string flag of <c>IStatus.GetAsync</c>. Pass an empty instance (<c>new()</c>) for the server default.
/// </summary>
public sealed class PlatformStatusQuery
{
	/// <summary>
	/// Whether to also include the cluster state and the database schema version of each module, sent as <c>verbose</c> (lowercase
	/// <c>true</c>/<c>false</c>); left out when <see langword="null"/> (the server default is <c>false</c>).
	/// </summary>
	[AliasAs("verbose")]
	public bool? Verbose { get; set; }
}