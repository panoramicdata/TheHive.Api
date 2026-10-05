using Refit;
using TheHive.Api.Data.Timeline;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on custom events in a case timeline. Read the timeline itself with <see cref="ICases.GetTimelineAsync"/>.
/// All operations require <c>manageCustomEvent</c>.
/// </summary>
public interface ITimeline
{
	/// <summary>Adds a custom event to a case timeline.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The event to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created event.</returns>
	[Post("api/v1/case/{caseId}/customEvent")]
	Task<CustomEvent> CreateCustomEventAsync(string caseId, [Body] CustomEventCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Updates a custom event; only the properties set on the request change.</summary>
	/// <param name="eventId">The custom event ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/customEvent/{eventId}")]
	Task UpdateCustomEventAsync(string eventId, [Body] CustomEventUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a custom event.</summary>
	/// <param name="eventId">The custom event ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/customEvent/{eventId}")]
	Task DeleteCustomEventAsync(string eventId, CancellationToken cancellationToken);
}
