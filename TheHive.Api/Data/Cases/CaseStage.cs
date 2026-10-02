namespace TheHive.Api.Data.Cases;

/// <summary>The stage derived from a case's (configurable) status.</summary>
public enum CaseStage
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The case is new.</summary>
	New,

	/// <summary>The case is being worked on.</summary>
	InProgress,

	/// <summary>The case is closed.</summary>
	Closed
}
