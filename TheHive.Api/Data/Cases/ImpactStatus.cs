namespace TheHive.Api.Data.Cases;

/// <summary>Whether an incident had real consequences for the organization.</summary>
public enum ImpactStatus
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The incident caused damage or data loss.</summary>
	WithImpact,

	/// <summary>The incident was resolved without consequences.</summary>
	NoImpact,

	/// <summary>The concept does not apply to this case.</summary>
	NotApplicable
}
