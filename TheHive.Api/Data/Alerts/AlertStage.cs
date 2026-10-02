namespace TheHive.Api.Data.Alerts;

/// <summary>The stage derived from an alert's (configurable) status.</summary>
public enum AlertStage
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The alert is new.</summary>
	New,

	/// <summary>The alert is being triaged.</summary>
	InProgress,

	/// <summary>The alert is closed.</summary>
	Closed,

	/// <summary>The alert has been imported into a case.</summary>
	Imported
}
