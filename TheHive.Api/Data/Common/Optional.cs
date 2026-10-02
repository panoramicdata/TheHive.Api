namespace TheHive.Api.Data.Common;

/// <summary>
/// A request value that is either unset (omitted from the JSON body) or set, possibly to <see langword="null"/>
/// (sent as an explicit JSON <c>null</c>, which TheHive treats as "clear this field").
/// </summary>
/// <typeparam name="T">The value type; use a nullable type (for example <c>string?</c> or <c>DateTimeOffset?</c>) so the value can be cleared.</typeparam>
public readonly struct Optional<T> : IOptional, IEquatable<Optional<T>>
{
	/// <summary>Creates a set value.</summary>
	/// <param name="value">The value, which may be <see langword="null"/>.</param>
	public Optional(T value)
	{
		HasValue = true;
		Value = value;
	}

	/// <summary>An unset value: the property is omitted from the request.</summary>
	public static Optional<T> Unset => default;

	/// <summary>Whether a value (possibly <see langword="null"/>) was set.</summary>
	public bool HasValue { get; }

	/// <summary>The value; <see langword="default"/> when <see cref="HasValue"/> is <see langword="false"/>.</summary>
	public T Value { get; }

	/// <summary>Sets a value, so <c>request.Summary = null</c> sends <c>"summary":null</c>.</summary>
	/// <param name="value">The value, which may be <see langword="null"/>.</param>
	public static implicit operator Optional<T>(T value) => new(value);

	/// <summary>Whether two values are equal: both unset, or both set to equal values.</summary>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	/// <returns><see langword="true"/> when equal.</returns>
	public static bool operator ==(Optional<T> left, Optional<T> right) => left.Equals(right);

	/// <summary>Whether two values differ.</summary>
	/// <param name="left">The first value.</param>
	/// <param name="right">The second value.</param>
	/// <returns><see langword="true"/> when not equal.</returns>
	public static bool operator !=(Optional<T> left, Optional<T> right) => !left.Equals(right);

	/// <summary>Whether this value equals <paramref name="other"/>: both unset, or both set to equal values (an unset value never equals a set <see langword="null"/>).</summary>
	/// <param name="other">The value to compare with.</param>
	/// <returns><see langword="true"/> when equal.</returns>
	public bool Equals(Optional<T> other) => HasValue == other.HasValue && EqualityComparer<T>.Default.Equals(Value, other.Value);

	/// <inheritdoc />
	public override bool Equals(object? obj) => obj is Optional<T> other && Equals(other);

	/// <inheritdoc />
	public override int GetHashCode() => HashCode.Combine(HasValue, Value);

	/// <summary>Describes the value: <c>Unset</c>, <c>Set(null)</c> or <c>Set(value)</c>.</summary>
	/// <returns>The description.</returns>
	public override string ToString() => !HasValue ? "Unset" : Value is null ? "Set(null)" : $"Set({Value})";
}
