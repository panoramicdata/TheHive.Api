using Refit;
using TheHive.Api.Data.Tags;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on tags. To list tags use the query API with <c>listTag</c> (custom tags) or <c>listTaxonomy</c> then <c>tags</c>
/// (taxonomy tags). Taxonomy tags cannot be updated or deleted. The spec has no create operation: custom tags are created when
/// they are first added to a case, alert or observable.
/// </summary>
public interface ITags
{
	/// <summary>Gets a tag.</summary>
	/// <param name="tagId">The tag ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The tag.</returns>
	[Get("api/v1/tag/{tagId}")]
	Task<Tag> GetAsync(string tagId, CancellationToken cancellationToken = default);

	/// <summary>Updates a custom tag everywhere it is used in the organization (requires <c>manageTag</c>); only the properties set on the request change.</summary>
	/// <param name="tagId">The tag ID preceded by <c>~</c>, or the tag name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/tag/{tagId}")]
	Task UpdateAsync(string tagId, [Body] TagUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a custom tag, removing it from all cases, alerts and observables in the organization (requires <c>manageTag</c>).</summary>
	/// <param name="tagId">The tag ID preceded by <c>~</c>, or the tag name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/tag/{tagId}")]
	Task DeleteAsync(string tagId, CancellationToken cancellationToken = default);
}
