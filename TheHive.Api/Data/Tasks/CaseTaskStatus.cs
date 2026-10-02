namespace TheHive.Api.Data.Tasks;

/// <summary>The status of a case task.</summary>
public enum CaseTaskStatus
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The task has not started.</summary>
	Waiting,

	/// <summary>The task is being worked on.</summary>
	InProgress,

	/// <summary>The task is complete.</summary>
	Completed,

	/// <summary>The task was cancelled.</summary>
	Cancel
}
