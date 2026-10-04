using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Converters;

/// <summary>
/// Converts <see cref="Optional{T}"/> as its inner value. Unset properties are omitted by the
/// <see cref="TheHiveJson.Options"/> type-info modifier, so a set <see langword="null"/> is written as JSON <c>null</c>.
/// </summary>
public sealed class OptionalConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert)
		=> typeToConvert.IsGenericType && typeToConvert.GetGenericTypeDefinition() == typeof(Optional<>);

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> (JsonConverter)Activator.CreateInstance(typeof(OptionalConverter<>).MakeGenericType(typeToConvert.GetGenericArguments()[0]))!;

	private sealed class OptionalConverter<T> : JsonConverter<Optional<T>>
	{
		public override Optional<T> Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
			=> new(JsonSerializer.Deserialize<T>(ref reader, options)!);

		public override void Write(Utf8JsonWriter writer, Optional<T> value, JsonSerializerOptions options)
			=> JsonSerializer.Serialize(writer, value.Value, options);
	}
}
