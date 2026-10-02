namespace TheHive.Api.Data.Common;

/// <summary>Named values for the numeric Traffic Light Protocol (TLP) levels TheHive sends and receives as integers.</summary>
public static class Tlp
{
	/// <summary>TLP:CLEAR (0): no restrictions.</summary>
	public const int Clear = 0;

	/// <summary>TLP:GREEN (1): community.</summary>
	public const int Green = 1;

	/// <summary>TLP:AMBER (2): organization and clients; the server default.</summary>
	public const int Amber = 2;

	/// <summary>TLP:AMBER+STRICT (3): organization only.</summary>
	public const int AmberStrict = 3;

	/// <summary>TLP:RED (4): named recipients only.</summary>
	public const int Red = 4;
}
