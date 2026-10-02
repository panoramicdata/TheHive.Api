namespace TheHive.Api.Data.Common;

/// <summary>
/// A request value that is either unset (omitted from the JSON body) or set, possibly to <see langword="null"/>
/// (sent as an explicit JSON <c>null</c>, which TheHive treats as "clear this field").
/// </summary>
/// <typeparam name="T">The value type; use a nullable type (for example <c>string?</c> or <c>DateTimeOffset?</c>) so the value can be cleared.</typeparam>
public readonly struct Optional<T> : IOptional
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
}
