using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The name of an email intake provider in the providers list (the spec's <c>EmailIntakeProviderName</c>).</summary>
public enum EmailIntakeProviderName
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>A generic IMAP server.</summary>
	[JsonStringEnumMemberName("imap")]
	Imap,

	/// <summary>Microsoft 365 (OAuth 2.0, Platinum licence).</summary>
	[JsonStringEnumMemberName("office365")]
	Office365,

	/// <summary>Google Workspace (OAuth 2.0, Platinum licence).</summary>
	[JsonStringEnumMemberName("google-workspace")]
	GoogleWorkspace,

	/// <summary>Microsoft Graph API (OAuth 2.0, Platinum licence).</summary>
	[JsonStringEnumMemberName("MSGraph365")]
	MsGraph365
}
