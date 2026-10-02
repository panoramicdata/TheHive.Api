using Refit;
using TheHive.Api.Data.Cases;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on TheHive cases.</summary>
public interface ICases
{
	/// <summary>Creates a case.</summary>
	/// <param name="request">The case to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created case.</returns>
	[Post("api/v1/case")]
	Task<Case> CreateAsync([Body] CaseCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets a case.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The case.</returns>
	[Get("api/v1/case/{idOrName}")]
	Task<Case> GetAsync(string idOrName, CancellationToken cancellationToken = default);

	/// <summary>Updates the fields set on <paramref name="request"/>; other fields keep their values.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The fields to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/{idOrName}")]
	Task UpdateAsync(string idOrName, [Body] CaseUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes a case. This cannot be undone.</summary>
	/// <param name="idOrName">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{idOrName}")]
	Task DeleteAsync(string idOrName, CancellationToken cancellationToken = default);

	/// <summary>Merges two or more cases into a new case; the source cases are deleted.</summary>
	/// <param name="ids">The comma-separated case IDs (each preceded by <c>~</c>) or case numbers, for example <c>~1,~2</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new case created by the merge.</returns>
	[Post("api/v1/case/_merge/{ids}")]
	Task<Case> MergeAsync(string ids, CancellationToken cancellationToken = default);
}
