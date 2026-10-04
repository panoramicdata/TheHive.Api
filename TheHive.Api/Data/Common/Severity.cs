namespace TheHive.Api.Data.Common;

/// <summary>Named values for TheHive's numeric severity levels (sent and received as integers).</summary>
public static class Severity
{
	/// <summary>Low severity (1).</summary>
	public static readonly int Low = 1;

	/// <summary>Medium severity (2), the server default.</summary>
	public static readonly int Medium = 2;

	/// <summary>High severity (3).</summary>
	public static readonly int High = 3;

	/// <summary>Critical severity (4).</summary>
	public static readonly int Critical = 4;
}
