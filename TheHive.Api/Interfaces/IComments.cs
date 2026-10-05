using Refit;
using TheHive.Api.Data.Comments;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on comments on cases and alerts.</summary>
public interface IComments
{
	/// <summary>Adds a comment to a case; it is visible to every organization sharing the case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created comment.</returns>
	[Post("api/v1/case/{caseId}/comment")]
	Task<Comment> AddToCaseAsync(string caseId, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Adds a comment to an alert.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The comment.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created comment.</returns>
	[Post("api/v1/alert/{alertId}/comment")]
	Task<Comment> AddToAlertAsync(string alertId, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Updates the text or portal visibility of a comment; only its author can change the text.</summary>
	/// <param name="commentId">The comment ID preceded by <c>~</c>.</param>
	/// <param name="request">The new text and visibility.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/comment/{commentId}")]
	Task UpdateAsync(string commentId, [Body] CommentRequest request, CancellationToken cancellationToken);

	/// <summary>Deletes a comment from a case or alert.</summary>
	/// <param name="commentId">The comment ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/comment/{commentId}")]
	Task DeleteAsync(string commentId, CancellationToken cancellationToken);
}
