namespace TheHive.Api.Data.Common;

/// <summary>Lets the serializer ask any <see cref="Optional{T}"/> whether it is set.</summary>
internal interface IOptional
{
	/// <summary>Whether a value was set.</summary>
	bool HasValue { get; }
}
