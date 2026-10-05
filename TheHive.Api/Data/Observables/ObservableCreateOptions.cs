using Refit;

namespace TheHive.Api.Data.Observables;

/// <summary>
/// The optional query-string parameter of <see cref="Interfaces.IObservables.CreateInCaseAsync"/> and <see cref="Interfaces.IObservables.CreateInAlertAsync"/>. Pass an empty instance
/// (<c>new()</c>) to leave it out.
/// </summary>
public sealed class ObservableCreateOptions
{
	/// <summary>
	/// The observable type, used by the server only when the request's <c>DataType</c> is missing, sent as <c>dataType</c>; left out when
	/// <see langword="null"/>.
	/// </summary>
	[AliasAs("dataType")]
	public string? DataType { get; set; }
}