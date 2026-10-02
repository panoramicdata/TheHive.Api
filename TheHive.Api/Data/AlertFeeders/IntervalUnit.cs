namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The unit of an <see cref="Interval"/> (the spec's <c>IntervalUnit</c>).</summary>
public enum IntervalUnit
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Days.</summary>
	Days,

	/// <summary>Hours.</summary>
	Hours,

	/// <summary>Minutes.</summary>
	Minutes,

	/// <summary>Seconds.</summary>
	Seconds,

	/// <summary>Milliseconds.</summary>
	Milliseconds
}
