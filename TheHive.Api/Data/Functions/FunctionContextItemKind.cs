namespace TheHive.Api.Data.Functions;

/// <summary>Whether a <see cref="FunctionContextItem"/> is a field or a method.</summary>
public enum FunctionContextItemKind
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>A field of the context object.</summary>
	Field,

	/// <summary>A method of the context object.</summary>
	Method
}
