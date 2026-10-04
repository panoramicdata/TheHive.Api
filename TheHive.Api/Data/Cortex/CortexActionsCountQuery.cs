using Refit;

namespace TheHive.Api.Data.Cortex;

/// <summary>
/// The optional query-string parameter of <see cref="Interfaces.ICortex.CountActionsAsync"/>. Pass an empty instance (<c>new()</c>) to count every action.
/// </summary>
public sealed class CortexActionsCountQuery
{
	/// <summary>A filter as JSON text, as for <see cref="CortexActionsQuery.Filter"/>, sent as <c>filter</c>; left out when <see langword="null"/>.</summary>
	[AliasAs("filter")]
	public string? Filter { get; set; }
}