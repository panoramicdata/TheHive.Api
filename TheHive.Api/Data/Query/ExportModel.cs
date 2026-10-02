namespace TheHive.Api.Data.Query;

/// <summary>The entity model of a query export; the wire names are the member names.</summary>
public enum ExportModel
{
	/// <summary>A value this client does not recognise; do not send it.</summary>
	Unknown = 0,

	/// <summary>Cases.</summary>
	Case,

	/// <summary>Alerts.</summary>
	Alert,

	/// <summary>Users.</summary>
	User,

	/// <summary>Organizations.</summary>
	Organisation,

	/// <summary>Procedures (TTPs).</summary>
	Procedure,

	/// <summary>Tasks.</summary>
	Task,

	/// <summary>Observables (the only model that supports <see cref="ExportFormat.Txt"/> and <see cref="ExportFormat.Misp"/>).</summary>
	Observable
}
