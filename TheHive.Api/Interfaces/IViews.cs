using Refit;
using TheHive.Api.Data.Views;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on saved list views (filters, sort order, columns and display options of a list). A view is private to its creator or
/// shared with the organization. For the entities <c>User</c>, <c>Organisation</c>, <c>Profiles</c>, <c>CustomFields</c>,
/// <c>ObservableTypes</c>, <c>Taxonomies</c> and <c>AttackPatterns</c>, send the <c>X-Organisation: admin</c> header (set
/// <see cref="TheHiveClientOptions.Organisation"/> to <c>admin</c>) to target the admin organization.
/// </summary>
public interface IViews
{
	/// <summary>Creates a view.</summary>
	/// <param name="request">The view to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created view.</returns>
	[Post("api/v1/views")]
	Task<View> CreateAsync([Body] ViewCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Gets a view.</summary>
	/// <param name="viewsId">The view ID, with or without the leading <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The view.</returns>
	[Get("api/v1/views/{viewsId}")]
	Task<View> GetAsync(string viewsId, CancellationToken cancellationToken);

	/// <summary>Updates a view; only the properties set on the request change.</summary>
	/// <param name="viewsId">The view ID, with or without the leading <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/views/{viewsId}")]
	Task UpdateAsync(string viewsId, [Body] ViewUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a view.</summary>
	/// <param name="viewsId">The view ID, with or without the leading <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/views/{viewsId}")]
	Task DeleteAsync(string viewsId, CancellationToken cancellationToken);
}
