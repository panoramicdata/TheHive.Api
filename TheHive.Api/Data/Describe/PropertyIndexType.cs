using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Describe;

/// <summary>How a model property is indexed (the spec's <c>AnyIndexType</c> and <c>BasicIndexType</c>).</summary>
public enum PropertyIndexType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Exact-match filtering, sorting and aggregation.</summary>
	[JsonStringEnumMemberName("standard")]
	Standard,

	/// <summary>Not indexed: the property cannot be filtered, sorted or aggregated.</summary>
	[JsonStringEnumMemberName("none")]
	None,

	/// <summary>Full-text search on top of the <see cref="Standard"/> capabilities.</summary>
	[JsonStringEnumMemberName("fulltext")]
	Fulltext,

	/// <summary>Full-text search only, without exact-match filtering, sorting or aggregation.</summary>
	[JsonStringEnumMemberName("fulltextOnly")]
	FulltextOnly
}
