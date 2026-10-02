using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>Whether a proxy is enabled, turned off, or uses the default system setting (the spec's <c>State</c>).</summary>
public enum ClientProxyState
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The proxy is used.</summary>
	[JsonStringEnumMemberName("enabled")]
	Enabled,

	/// <summary>The proxy is not used.</summary>
	[JsonStringEnumMemberName("disabled")]
	Disabled,

	/// <summary>The default system setting applies.</summary>
	[JsonStringEnumMemberName("default")]
	Default
}
