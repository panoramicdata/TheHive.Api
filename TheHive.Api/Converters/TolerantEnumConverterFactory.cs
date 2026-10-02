using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Converters;

/// <summary>Reads enums case-insensitively, mapping unrecognised names to the default value (<c>Unknown</c>).</summary>
public sealed class TolerantEnumConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert))!;

	private sealed class TolerantEnumConverter<T> : JsonConverter<T> where T : struct, Enum
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.TokenType == JsonTokenType.String
				? (Enum.TryParse<T>(reader.GetString(), true, out var value) ? value : default)
				: throw new JsonException("Expected an enum name string.");

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString());
	}
}
