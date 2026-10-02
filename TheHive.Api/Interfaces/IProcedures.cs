using Refit;
using TheHive.Api.Data.Procedures;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on procedures (TTPs): links between an ATT&amp;CK technique and a case or alert, with the date it occurred and how it was carried out
/// (the spec tag <c>TTP</c>). All writes require <c>manageProcedure</c>. To list procedures use the query API with <c>listProcedure</c>.
/// </summary>
public interface IProcedures
{
	/// <summary>Adds a procedure to an alert.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The procedure to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created procedure.</returns>
	[Post("api/v1/alert/{alertId}/procedure")]
	Task<Procedure> CreateForAlertAsync(string alertId, [Body] ProcedureInput request, CancellationToken cancellationToken = default);

	/// <summary>Adds several procedures to an alert in one request.</summary>
	/// <param name="alertId">The alert ID preceded by <c>~</c>.</param>
	/// <param name="request">The procedures to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created procedures.</returns>
	[Post("api/v1/alert/{alertId}/procedures")]
	Task<List<Procedure>> CreateManyForAlertAsync(string alertId, [Body] ProcedureBulkCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Adds a procedure to a case.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The procedure to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created procedure.</returns>
	[Post("api/v1/case/{caseId}/procedure")]
	Task<Procedure> CreateForCaseAsync(string caseId, [Body] ProcedureInput request, CancellationToken cancellationToken = default);

	/// <summary>Adds several procedures to a case in one request.</summary>
	/// <param name="caseId">The case ID preceded by <c>~</c>, or the case number.</param>
	/// <param name="request">The procedures to add.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created procedures.</returns>
	[Post("api/v1/case/{caseId}/procedures")]
	Task<List<Procedure>> CreateManyForCaseAsync(string caseId, [Body] ProcedureBulkCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes a procedure from a case or alert.</summary>
	/// <param name="procedureId">The procedure ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/procedure/{procedureId}")]
	Task DeleteAsync(string procedureId, CancellationToken cancellationToken = default);

	/// <summary>Updates a procedure; only the properties set on the request change.</summary>
	/// <param name="procedureId">The procedure ID preceded by <c>~</c>.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/procedure/{procedureId}")]
	Task UpdateAsync(string procedureId, [Body] ProcedureUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Permanently deletes several procedures in one request.</summary>
	/// <param name="request">The IDs of the procedures to delete.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/procedure/delete/_bulk")]
	Task BulkDeleteAsync([Body] ProcedureBulkDeleteRequest request, CancellationToken cancellationToken = default);
}
