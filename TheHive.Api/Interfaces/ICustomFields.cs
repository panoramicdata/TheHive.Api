using Refit;
using TheHive.Api.Data.CustomFields;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on custom field definitions. Writes require <c>manageCustomField</c>; send the <c>X-Organisation: admin</c> header to target the admin organization.</summary>
public interface ICustomFields
{
	/// <summary>Lists every custom field definition.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The custom fields.</returns>
	[Get("api/v1/customField")]
	Task<List<CustomField>> ListAsync(CancellationToken cancellationToken = default);

	/// <summary>Creates a custom field.</summary>
	/// <param name="request">The custom field to create.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created custom field.</returns>
	[Post("api/v1/customField")]
	Task<CustomField> CreateAsync([Body] CustomFieldCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a custom field.</summary>
	/// <param name="customFieldId">The custom field ID preceded by <c>~</c>, or its name.</param>
	/// <param name="options">The <c>force</c> flag (<see cref="CustomFieldDeleteOptions.Force"/>: whether to delete the field even when cases or alerts use it); pass <c>new()</c> to leave it out, and the server then refuses to delete a field in use.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/customField/{customFieldId}")]
	Task DeleteAsync(string customFieldId, [Query] CustomFieldDeleteOptions options, CancellationToken cancellationToken = default);

	/// <summary>Updates a custom field; only the properties set on the request change.</summary>
	/// <param name="customFieldId">The custom field ID preceded by <c>~</c>, or its name.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/customField/{customFieldId}")]
	Task UpdateAsync(string customFieldId, [Body] CustomFieldUpdateRequest request, CancellationToken cancellationToken = default);
}
