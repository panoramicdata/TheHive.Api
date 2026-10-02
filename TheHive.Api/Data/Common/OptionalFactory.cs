namespace TheHive.Api.Data.Common;

/// <summary>Factory methods for <see cref="Optional{T}"/>.</summary>
public static class Optional
{
	/// <summary>Creates a set value; <see langword="null"/> clears the field on the server.</summary>
	/// <typeparam name="T">The value type.</typeparam>
	/// <param name="value">The value, which may be <see langword="null"/>.</param>
	/// <returns>A set <see cref="Optional{T}"/>.</returns>
	public static Optional<T> Of<T>(T value) => new(value);
}
