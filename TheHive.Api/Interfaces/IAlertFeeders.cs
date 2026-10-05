using Refit;
using TheHive.Api.Data.AlertFeeders;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Functions;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on alert feeders, which periodically request data from an external system over HTTP and convert it into alerts with a
/// <c>feeder:alert</c> function (Platinum licence; all need <c>manageConfig</c>). Feeder configurations hold credentials (authentication,
/// header values, proxy passwords): they only ever travel in request and response bodies, never in a URL.
/// </summary>
public interface IAlertFeeders
{
	/// <summary>Lists the alert feeders configured in the current organization.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The alert feeders.</returns>
	[Get("api/v1/connector/alert-feeder")]
	Task<List<AlertFeeder>> ListAsync(CancellationToken cancellationToken);

	/// <summary>Creates an alert feeder. Its feeder function must already exist and have the type <c>feeder:alert</c> (see <see cref="IFunctions.CreateAsync"/>).</summary>
	/// <param name="request">The alert feeder to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created alert feeder.</returns>
	[Post("api/v1/connector/alert-feeder")]
	Task<AlertFeeder> CreateAsync([Body] AlertFeederCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes an alert feeder; its feeder function is not deleted (see <see cref="IFunctions.DeleteAsync"/>).</summary>
	/// <param name="alertFeederName">The name of the alert feeder.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/connector/alert-feeder/{alertFeederName}")]
	Task DeleteAsync(string alertFeederName, CancellationToken cancellationToken);

	/// <summary>Updates an alert feeder.</summary>
	/// <param name="alertFeederName">The name of the alert feeder.</param>
	/// <param name="request">The new configuration.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The updated alert feeder (the spec answers 201).</returns>
	[Put("api/v1/connector/alert-feeder/{alertFeederName}")]
	Task<AlertFeeder> UpdateAsync(string alertFeederName, [Body] AlertFeederUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Runs an alert feeder immediately, outside its schedule.</summary>
	/// <param name="alertFeederName">The name of the alert feeder.</param>
	/// <param name="options">The <c>dryRun</c> flag (<see langword="true"/> to simulate the run without creating alerts; omit for the server default, <see langword="false"/>); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The function result: the returned value, duration, stdout and stderr.</returns>
	[Post("api/v1/connector/alert-feeder/run/{alertFeederName}")]
	Task<FunctionInvocationResult> RunAsync(string alertFeederName, [Query] DryRunOptions options, CancellationToken cancellationToken);

	/// <summary>Tests the HTTP connection of an alert feeder configuration without saving it.</summary>
	/// <param name="request">The configuration to test.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The raw response body from the external system. The spec declares an untyped JSON response, so it is returned as text for the caller to parse.</returns>
	[Post("api/v1/connector/alert-feeder/test")]
	Task<string> TestAsync([Body] AlertFeederTestRequest request, CancellationToken cancellationToken);
}
