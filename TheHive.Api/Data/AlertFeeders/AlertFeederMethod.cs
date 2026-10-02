using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The HTTP method an alert feeder uses to request data (the spec's <c>Method</c>).</summary>
public enum AlertFeederMethod
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary><c>GET</c>.</summary>
	[JsonStringEnumMemberName("GET")]
	Get,

	/// <summary><c>POST</c>; the request <c>body</c> is sent.</summary>
	[JsonStringEnumMemberName("POST")]
	Post
}
