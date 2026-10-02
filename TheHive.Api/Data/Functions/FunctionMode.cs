namespace TheHive.Api.Data.Functions;

/// <summary>The execution mode of a function when sent in a request (the spec's <c>InputFunctionMode</c>).</summary>
public enum FunctionMode
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>The function runs normally.</summary>
	Enabled,

	/// <summary>The function does not run.</summary>
	Disabled,

	/// <summary>The function runs without persisting any changes.</summary>
	DryRun
}
