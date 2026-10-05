using Refit;
using TheHive.Api.Data.Pages;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on page templates: pages that are copied into a case when a case template linking them is applied.
/// The spec reuses the page schemas, so they share the page types. Link templates to a case template with
/// <see cref="ICaseTemplates"/>.
/// </summary>
public interface IPageTemplates
{
	/// <summary>Creates a page template (requires <c>managePageTemplate</c>).</summary>
	/// <param name="request">The page template to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created page template.</returns>
	[Post("api/v1/pageTemplate")]
	Task<Page> CreateAsync([Body] PageCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Updates a page template (requires <c>managePageTemplate</c>); only the properties set on the request change.</summary>
	/// <param name="pageTemplateId">The page template ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/pageTemplate/{pageTemplateId}")]
	Task UpdateAsync(string pageTemplateId, [Body] PageUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a page template (requires <c>managePageTemplate</c>).</summary>
	/// <param name="pageTemplateId">The page template ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/pageTemplate/{pageTemplateId}")]
	Task DeleteAsync(string pageTemplateId, CancellationToken cancellationToken);
}
