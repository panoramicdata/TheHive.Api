namespace TheHive.Api.Data.EmailIntake;

/// <summary>The known values of <see cref="EmailIntakeMailbox.Kind"/>, the <c>_kind</c> discriminator of the spec's mailbox schemas.</summary>
public static class EmailIntakeMailboxKinds
{
	/// <summary>An API-based mailbox (Microsoft 365, Microsoft Graph API, Google Workspace).</summary>
	public static readonly string Api = "api";

	/// <summary>An IMAP mailbox.</summary>
	public static readonly string Imap = "imap";
}
