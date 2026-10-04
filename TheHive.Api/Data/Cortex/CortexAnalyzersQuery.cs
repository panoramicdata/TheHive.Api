using Refit;

namespace TheHive.Api.Data.Cortex;

/// <summary>
/// The optional query-string parameter of <see cref="Interfaces.ICortex.ListAnalyzersAsync"/>. Pass an empty instance (<c>new()</c>) for Cortex's default range.
/// </summary>
public sealed class CortexAnalyzersQuery
{
	/// <summary>
	/// <c>all</c> for every analyzer, or Cortex's <c>start-end</c> range such as <c>0-25</c>, sent as <c>range</c>; left out when <see langword="null"/>,
	/// for Cortex's default <c>0-10</c>.
	/// </summary>
	[AliasAs("range")]
	public string? Range { get; set; }
}