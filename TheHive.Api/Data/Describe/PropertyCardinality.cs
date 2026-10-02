using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Describe;

/// <summary>How many values a model property holds (the spec's <c>Cardinality</c>).</summary>
public enum PropertyCardinality
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Exactly one value (mandatory).</summary>
	[JsonStringEnumMemberName("single")]
	Single,

	/// <summary>Zero or one value.</summary>
	[JsonStringEnumMemberName("option")]
	Option,

	/// <summary>Zero or more values, duplicates allowed.</summary>
	[JsonStringEnumMemberName("list")]
	List,

	/// <summary>Zero or more unique values.</summary>
	[JsonStringEnumMemberName("set")]
	Set
}
