using Refit;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Functions;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on functions: JavaScript code that processes data and calls TheHive API with the permissions of the invoking user
/// (Platinum licence, except <see cref="GetContextDocumentationAsync"/>). Function <c>config</c> objects can hold secrets; they only ever
/// travel in request and response bodies.
/// </summary>
public interface IFunctions
{
	/// <summary>Creates a function. Requires <c>manageFunction/create</c>.</summary>
	/// <param name="request">The function to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created function.</returns>
	[Post("api/v1/function")]
	Task<Function> CreateAsync([Body] FunctionCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Lists the fields and methods available on the <c>context</c> object passed to function code.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The context documentation.</returns>
	[Get("api/v1/function/_context/documentation")]
	Task<FunctionContextDocumentation> GetContextDocumentationAsync(CancellationToken cancellationToken);

	/// <summary>Tests function code without saving it and returns the same output as invoking a saved function. Requires <c>manageFunction/create</c>.</summary>
	/// <param name="request">The code, configuration and input to run.</param>
	/// <param name="options">The <c>dryRun</c> flag (<see langword="true"/> to run without persisting any changes; omit for the server default, <see langword="false"/>); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The returned value, duration, stdout and stderr.</returns>
	[Post("api/v1/function/_test")]
	Task<FunctionInvocationResult> TestAsync([Body] FunctionTestRequest request, [Query] DryRunOptions options, CancellationToken cancellationToken);

	/// <summary>Invokes a function, optionally passing a JSON payload as input. Requires <c>manageFunction/invoke</c>.</summary>
	/// <param name="function">The function ID preceded by <c>~</c>, or the function name.</param>
	/// <param name="input">The payload passed to <c>handle</c>: any JSON-serializable value. A <see langword="null"/> input is sent as the JSON literal <c>null</c>.</param>
	/// <param name="options">The <c>dryRun</c> flag (<see langword="true"/> to run without persisting any changes; omit for the server default, <see langword="false"/>); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The returned value, duration, stdout and stderr.</returns>
	[Post("api/v1/function/{function}")]
	Task<FunctionInvocationResult> InvokeAsync(string function, [Body] object? input, [Query] DryRunOptions options, CancellationToken cancellationToken);

	/// <summary>Invokes a function of type <c>action:case</c> or <c>action:alert</c> on a case or alert, passing the object as input. Requires <c>manageFunction/invoke</c>.</summary>
	/// <param name="function">The function ID preceded by <c>~</c>, or the function name.</param>
	/// <param name="objectType"><c>case</c> or <c>alert</c>; it must match the function's type.</param>
	/// <param name="objectIdOrName">The object ID preceded by <c>~</c>, the case number for a case, or the alert reference (<c>type;source;sourceRef</c>) for an alert.</param>
	/// <param name="options">The <c>dryRun</c> and <c>sync</c> flags; a <see langword="null"/> flag is left out of the query string; pass <c>new()</c> for neither.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The result when run synchronously (<see cref="FunctionInvocationOptions.Sync"/>); an empty result (see <see cref="FunctionInvocationResult"/>) for a background run.</returns>
	[Post("api/v1/function/{function}/{objectType}/{objectIdOrName}")]
	Task<FunctionInvocationResult> InvokeOnObjectAsync(
		string function,
		string objectType,
		string objectIdOrName,
		[Query] FunctionInvocationOptions options,
		CancellationToken cancellationToken);

	/// <summary>Deletes a function. Requires <c>manageFunction/create</c>.</summary>
	/// <param name="functionId">The function ID preceded by <c>~</c>, or the function name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/function/{functionId}")]
	Task DeleteAsync(string functionId, CancellationToken cancellationToken);

	/// <summary>Gets a function.</summary>
	/// <param name="functionId">The function ID preceded by <c>~</c>, or the function name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The function.</returns>
	[Get("api/v1/function/{functionId}")]
	Task<Function> GetAsync(string functionId, CancellationToken cancellationToken);

	/// <summary>Updates one or more fields of a function. Requires <c>manageFunction/create</c>.</summary>
	/// <param name="functionId">The function ID preceded by <c>~</c>, or the function name.</param>
	/// <param name="request">The fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/function/{functionId}")]
	Task UpdateAsync(string functionId, [Body] FunctionUpdateRequest request, CancellationToken cancellationToken);
}
