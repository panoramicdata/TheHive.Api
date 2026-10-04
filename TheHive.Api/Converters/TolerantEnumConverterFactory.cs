using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Converters;

/// <summary>
/// Reads enums case-insensitively, mapping unrecognised names to the default value (<c>Unknown</c>).
/// Honours <see cref="JsonStringEnumMemberNameAttribute"/> for members whose wire name differs from the C# name.
/// </summary>
public sealed class TolerantEnumConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert))!;

	private sealed class TolerantEnumConverter<T> : JsonConverter<T> where T : struct, Enum
	{
		private readonly Dictionary<string, T> _byWireName = new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<T, string> _wireNames = [];

		public TolerantEnumConverter()
		{
			foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				var value = (T)field.GetValue(null)!;
				var wireName = field.GetCustomAttribute<JsonStringEnumMemberNameAttribute>()?.Name ?? field.Name;
				_byWireName[wireName] = value;
				_wireNames[value] = wireName;
			}
		}

		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			_ = (typeToConvert, options);
			return reader.TokenType == JsonTokenType.String
				? _byWireName.GetValueOrDefault(reader.GetString()!)
				: throw new JsonException("Expected an enum name string.");
		}

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
		{
			_ = options;
			writer.WriteStringValue(_wireNames.TryGetValue(value, out var wireName) ? wireName : value.ToString());
		}
	}
}
