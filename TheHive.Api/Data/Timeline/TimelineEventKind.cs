using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Timeline;

/// <summary>The type of a <see cref="TimelineEvent"/>.</summary>
public enum TimelineEventKind
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The incident started (<c>case.start</c>).</summary>
	[JsonStringEnumMemberName("case.start")]
	CaseStart,

	/// <summary>The case was created (<c>case.created</c>).</summary>
	[JsonStringEnumMemberName("case.created")]
	CaseCreated,

	/// <summary>The case entered the New stage (<c>case.new</c>).</summary>
	[JsonStringEnumMemberName("case.new")]
	CaseNew,

	/// <summary>The case entered the In progress stage (<c>case.inProgress</c>).</summary>
	[JsonStringEnumMemberName("case.inProgress")]
	CaseInProgress,

	/// <summary>The case was closed (<c>case.closed</c>).</summary>
	[JsonStringEnumMemberName("case.closed")]
	CaseClosed,

	/// <summary>The incident ended (<c>case.end</c>).</summary>
	[JsonStringEnumMemberName("case.end")]
	CaseEnd,

	/// <summary>An alert occurred (<c>alert.occurred</c>).</summary>
	[JsonStringEnumMemberName("alert.occurred")]
	AlertOccurred,

	/// <summary>A TTP occurred (<c>procedure.occurred</c>).</summary>
	[JsonStringEnumMemberName("procedure.occurred")]
	ProcedureOccurred,

	/// <summary>An IOC was sighted (<c>observable.sighted</c>).</summary>
	[JsonStringEnumMemberName("observable.sighted")]
	ObservableSighted,

	/// <summary>Task activity (<c>task</c>).</summary>
	[JsonStringEnumMemberName("task")]
	Task,

	/// <summary>A flagged task log was created (<c>log.created</c>).</summary>
	[JsonStringEnumMemberName("log.created")]
	LogCreated,

	/// <summary>A custom event (<c>custom</c>).</summary>
	[JsonStringEnumMemberName("custom")]
	Custom
}
