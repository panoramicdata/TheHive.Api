using System.Text.Json.Serialization;

namespace TheHive.Api.Data.CustomFields;

/// <summary>The data type of a custom field definition (the spec's <c>CustomFieldType</c>).</summary>
public enum CustomFieldType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>A text value.</summary>
	[JsonStringEnumMemberName("string")]
	String,

	/// <summary>A whole number.</summary>
	[JsonStringEnumMemberName("integer")]
	Integer,

	/// <summary>A floating-point number.</summary>
	[JsonStringEnumMemberName("float")]
	Float,

	/// <summary>A true or false value.</summary>
	[JsonStringEnumMemberName("boolean")]
	Boolean,

	/// <summary>A date, sent as epoch milliseconds.</summary>
	[JsonStringEnumMemberName("date")]
	Date,

	/// <summary>A URL.</summary>
	[JsonStringEnumMemberName("url")]
	Url
}
