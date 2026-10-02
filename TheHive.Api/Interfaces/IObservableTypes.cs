using Refit;
using TheHive.Api.Data.ObservableTypes;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on observable types. Writes require <c>manageObservableTemplate</c>; call them with the <c>X-Organisation: admin</c> header to target the admin organization.</summary>
public interface IObservableTypes
{
	/// <summary>Creates a custom observable type.</summary>
	/// <param name="request">The type to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created type.</returns>
	[Post("api/v1/observable/type")]
	Task<ObservableType> CreateAsync([Body] ObservableTypeCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets an observable type.</summary>
	/// <param name="typeId">The observable type ID preceded by <c>~</c>, or the type name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The type.</returns>
	[Get("api/v1/observable/type/{typeId}")]
	Task<ObservableType> GetAsync(string typeId, CancellationToken cancellationToken = default);

	/// <summary>Updates the case sensitivity of an observable type; it applies to new observables only.</summary>
	/// <param name="typeId">The observable type ID preceded by <c>~</c>, or the type name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/observable/type/{typeId}")]
	Task UpdateAsync(string typeId, [Body] ObservableTypeUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes an observable type; it fails while an observable uses the type.</summary>
	/// <param name="typeId">The observable type ID preceded by <c>~</c>, or the type name.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/observable/type/{typeId}")]
	Task DeleteAsync(string typeId, CancellationToken cancellationToken = default);
}
