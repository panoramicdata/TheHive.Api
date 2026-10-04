namespace TheHive.Api.Data.Admin;

/// <summary>The log levels accepted by <see cref="Interfaces.IAdmin.SetLogLevelAsync"/> (the spec's <c>InputLogLevel</c>). They are passed as plain strings because they are written to the URL path exactly as shown.</summary>
public static class LogLevels
{
	/// <summary>Logs the most detailed information.</summary>
	public static readonly string All = "ALL";

	/// <summary>Logs the most detailed information.</summary>
	public static readonly string Trace = "TRACE";

	/// <summary>Logs debugging information.</summary>
	public static readonly string Debug = "DEBUG";

	/// <summary>Logs general information.</summary>
	public static readonly string Info = "INFO";

	/// <summary>Logs warnings and errors.</summary>
	public static readonly string Warn = "WARN";

	/// <summary>Logs errors only.</summary>
	public static readonly string Error = "ERROR";

	/// <summary>Turns logging off entirely.</summary>
	public static readonly string Off = "OFF";
}
