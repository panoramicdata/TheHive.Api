namespace TheHive.Api.Data.Admin;

/// <summary>The log levels accepted by <see cref="Interfaces.IAdmin.SetLogLevelAsync"/> (the spec's <c>InputLogLevel</c>). They are passed as plain strings because they are written to the URL path exactly as shown.</summary>
public static class LogLevels
{
	/// <summary>Logs the most detailed information.</summary>
	public const string All = "ALL";

	/// <summary>Logs the most detailed information.</summary>
	public const string Trace = "TRACE";

	/// <summary>Logs debugging information.</summary>
	public const string Debug = "DEBUG";

	/// <summary>Logs general information.</summary>
	public const string Info = "INFO";

	/// <summary>Logs warnings and errors.</summary>
	public const string Warn = "WARN";

	/// <summary>Logs errors only.</summary>
	public const string Error = "ERROR";

	/// <summary>Turns logging off entirely.</summary>
	public const string Off = "OFF";
}
