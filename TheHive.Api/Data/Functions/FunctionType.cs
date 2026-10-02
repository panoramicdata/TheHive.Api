using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Functions;

/// <summary>A function type, which determines how and when the function can be invoked (the spec's <c>InputFunctionType</c>).</summary>
public enum FunctionType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Invoked through the API.</summary>
	[JsonStringEnumMemberName("api")]
	Api,

	/// <summary>Invoked by a notification.</summary>
	[JsonStringEnumMemberName("notification")]
	Notification,

	/// <summary>A manual action on a case.</summary>
	[JsonStringEnumMemberName("action:case")]
	ActionCase,

	/// <summary>A manual action on an alert.</summary>
	[JsonStringEnumMemberName("action:alert")]
	ActionAlert,

	/// <summary>An alert feeder function, which converts an HTTP response into alerts.</summary>
	[JsonStringEnumMemberName("feeder:alert")]
	FeederAlert
}
