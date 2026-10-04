namespace TheHive.Api.Data.Common;

/// <summary>Named values for the numeric Permissible Actions Protocol (PAP) levels TheHive sends and receives as integers.</summary>
public static class Pap
{
	/// <summary>PAP:CLEAR (0): no restrictions.</summary>
	public static readonly int Clear = 0;

	/// <summary>PAP:GREEN (1): active actions allowed.</summary>
	public static readonly int Green = 1;

	/// <summary>PAP:AMBER (2): passive checks only; the server default.</summary>
	public static readonly int Amber = 2;

	/// <summary>PAP:RED (3): non-detectable actions only.</summary>
	public static readonly int Red = 3;
}
