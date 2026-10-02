using Refit;
using TheHive.Api.Data.CaseTemplates;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on case templates. Writes require <c>manageCaseTemplate</c>; to list templates use the query API with <c>listCaseTemplate</c>.</summary>
public interface ICaseTemplates
{
	/// <summary>Creates a case template in the organization.</summary>
	/// <param name="request">The template to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created template.</returns>
	[Post("api/v1/caseTemplate")]
	Task<CaseTemplate> CreateAsync([Body] CaseTemplateCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a case template; cases already created from it are not affected.</summary>
	/// <param name="caseTemplateNameOrId">The template ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/caseTemplate/{caseTemplateNameOrId}")]
	Task DeleteAsync(string caseTemplateNameOrId, CancellationToken cancellationToken = default);

	/// <summary>Gets a case template.</summary>
	/// <param name="caseTemplateNameOrId">The template ID preceded by <c>~</c>, or its name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The template.</returns>
	[Get("api/v1/caseTemplate/{caseTemplateNameOrId}")]
	Task<CaseTemplate> GetAsync(string caseTemplateNameOrId, CancellationToken cancellationToken = default);

	/// <summary>Updates a case template; only the properties set on the request change.</summary>
	/// <param name="caseTemplateNameOrId">The template ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/caseTemplate/{caseTemplateNameOrId}")]
	Task UpdateAsync(string caseTemplateNameOrId, [Body] CaseTemplateUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Replaces the full list of page templates linked to a case template.</summary>
	/// <param name="caseTemplateNameOrId">The template ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The page template IDs to link; an empty list removes all links.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/caseTemplate/{caseTemplateNameOrId}/pageTemplate/link")]
	Task LinkPageTemplatesAsync(string caseTemplateNameOrId, [Body] CaseTemplatePageLinkRequest request, CancellationToken cancellationToken = default);
}
