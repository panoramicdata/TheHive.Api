using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The connection kind of an email intake provider in the providers list (the spec's <c>EmailIntakeConfigKind</c>).</summary>
public enum EmailIntakeConfigKind
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>An API-based provider.</summary>
	[JsonStringEnumMemberName("api")]
	Api,

	/// <summary>An IMAP-based provider.</summary>
	[JsonStringEnumMemberName("imap")]
	Imap
}
