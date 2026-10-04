using Refit;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>
/// The optional query-string parameter of <c>IEmailIntake.SyncAsync</c>. Pass an empty instance (<c>new()</c>) to sync every connected mailbox.
/// </summary>
public sealed class EmailIntakeSyncOptions
{
	/// <summary>
	/// The ID of a single mailbox configuration to sync, sent as <c>configId</c>; left out when <see langword="null"/>, which syncs every connected
	/// mailbox. The spec describes this <c>configId</c> query parameter only in prose, not as a declared parameter.
	/// </summary>
	[AliasAs("configId")]
	public string? ConfigId { get; set; }
}