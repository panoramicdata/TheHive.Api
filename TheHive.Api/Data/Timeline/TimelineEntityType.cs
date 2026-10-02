namespace TheHive.Api.Data.Timeline;

/// <summary>The type of entity involved in a <see cref="TimelineEvent"/>.</summary>
public enum TimelineEntityType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>A case.</summary>
	Case,

	/// <summary>An alert.</summary>
	Alert,

	/// <summary>A procedure (TTP).</summary>
	Procedure,

	/// <summary>An observable.</summary>
	Observable,

	/// <summary>A task.</summary>
	Task,

	/// <summary>A task log.</summary>
	Log,

	/// <summary>A custom event.</summary>
	CustomEvent
}
