using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>How new tasks or observables are shared with sharing organizations.</summary>
public enum SharingRule
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>New items are shared automatically.</summary>
	[JsonStringEnumMemberName("autoShare")]
	AutoShare,

	/// <summary>Each item must be shared explicitly.</summary>
	[JsonStringEnumMemberName("manual")]
	Manual
}
