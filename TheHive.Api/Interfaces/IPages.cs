using Refit;
using TheHive.Api.Data.Pages;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on Knowledge Base pages (organization-wide Markdown pages) and case pages (temporary pages scoped to one case).
/// To list pages use the query API with <c>listOrganisationPage</c> (Knowledge Base) or <c>listPage</c> (categories).
/// Page templates are on <see cref="IPageTemplates"/>.
/// </summary>
public interface IPages
{
	/// <summary>Creates a Knowledge Base page in the organization (requires <c>manageKnowledgeBase</c>).</summary>
	/// <param name="request">The page to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created page.</returns>
	[Post("api/v1/page")]
	Task<Page> CreateAsync([Body] PageCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Updates a Knowledge Base page (requires <c>manageKnowledgeBase</c>); only the properties set on the request change.</summary>
	/// <param name="pageId">The page ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/page/{pageId}")]
	Task UpdateAsync(string pageId, [Body] PageUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a Knowledge Base page (requires <c>manageKnowledgeBase</c>).</summary>
	/// <param name="pageId">The page ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/page/{pageId}")]
	Task DeleteAsync(string pageId, CancellationToken cancellationToken = default);

	/// <summary>Creates a page in a case (requires <c>managePage</c>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The page to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created page.</returns>
	[Post("api/v1/case/{caseId}/page")]
	Task<Page> CreateInCaseAsync(string caseId, [Body] PageCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Updates a page in a case (requires <c>managePage</c>); only the properties set on the request change.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="pageId">The page ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/case/{caseId}/page/{pageId}")]
	Task UpdateInCaseAsync(string caseId, string pageId, [Body] PageUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a page from a case (requires <c>managePage</c>).</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="pageId">The page ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/case/{caseId}/page/{pageId}")]
	Task DeleteInCaseAsync(string caseId, string pageId, CancellationToken cancellationToken = default);
}
