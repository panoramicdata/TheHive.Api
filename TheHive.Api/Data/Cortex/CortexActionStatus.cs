namespace TheHive.Api.Data.Cortex;

/// <summary>The status of a <see cref="CortexAction"/> (the spec's <c>OutputAction.status</c> enum).</summary>
public enum CortexActionStatus
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The responder is running.</summary>
	InProgress,

	/// <summary>The responder finished successfully.</summary>
	Success,

	/// <summary>The responder failed.</summary>
	Failure,

	/// <summary>Cortex has not started the responder yet; the state of a newly created action.</summary>
	Waiting,

	/// <summary>The action was deleted.</summary>
	Deleted
}
