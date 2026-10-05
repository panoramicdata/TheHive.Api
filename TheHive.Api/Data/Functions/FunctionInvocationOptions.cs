using Refit;

namespace TheHive.Api.Data.Functions;

/// <summary>
/// The optional query-string flags of <see cref="Interfaces.IFunctions.InvokeOnObjectAsync"/>. Each property is sent as the query parameter of the same wire name,
/// as lowercase <c>true</c>/<c>false</c>, and is left out when <see langword="null"/>.
/// </summary>
public sealed class FunctionInvocationOptions
{
	/// <summary><see langword="true"/> to run without persisting any changes; omit for the server default (<see langword="false"/>). Sent as <c>dryRun</c>.</summary>
	[AliasAs("dryRun")]
	public bool? DryRun { get; set; }

	/// <summary><see langword="true"/> to wait for the function and return its result; omit or <see langword="false"/> to run in the background. Sent as <c>sync</c>.</summary>
	[AliasAs("sync")]
	public bool? Sync { get; set; }
}
