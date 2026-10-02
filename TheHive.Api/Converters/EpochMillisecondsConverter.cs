using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Converters;

/// <summary>Converts TheHive epoch-millisecond timestamps to and from <see cref="DateTimeOffset"/>.</summary>
public sealed class EpochMillisecondsConverter : JsonConverter<DateTimeOffset>
{
	/// <inheritdoc />
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.TokenType == JsonTokenType.Number
			? DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
			: throw new JsonException("Expected an epoch-millisecond number.");

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
		=> writer.WriteNumberValue(value.ToUnixTimeMilliseconds());
}
