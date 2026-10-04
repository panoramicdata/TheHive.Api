using Refit;

namespace TheHive.Api.Data.Common;

/// <summary>
/// The optional <c>dryRun</c> query-string flag of <see cref="Interfaces.IFunctions.TestAsync"/>, <see cref="Interfaces.IFunctions.InvokeAsync"/> and <see cref="Interfaces.IAlertFeeders.RunAsync"/>. Pass an
/// empty instance (<c>new()</c>) to leave it out.
/// </summary>
public sealed class DryRunOptions
{
	/// <summary>
	/// <see langword="true"/> to run without persisting any changes (for an alert feeder, to simulate the run without creating alerts); omit for the
	/// server default (<see langword="false"/>). Sent as <c>dryRun</c>, lowercase <c>true</c>/<c>false</c>, and left out when <see langword="null"/>.
	/// </summary>
	[AliasAs("dryRun")]
	public bool? DryRun { get; set; }
}