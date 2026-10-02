using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>
/// Reads a <see cref="LicenseList"/> from a JSON array, from an empty object (the spec's <c>Nil</c>, read as no licenses) or from JSON <c>null</c> (read as no licenses).
/// Any other object is rejected with a <see cref="JsonException"/>.
/// </summary>
internal sealed class LicenseListConverter : JsonConverter<LicenseList>
{
	/// <inheritdoc />
	public override bool HandleNull => true;

	/// <inheritdoc />
	public override LicenseList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var result = new LicenseList();
		switch (reader.TokenType)
		{
			case JsonTokenType.Null:
				return result;
			case JsonTokenType.StartObject:
				reader.Read();
				if (reader.TokenType != JsonTokenType.EndObject)
				{
					throw new JsonException("Expected a license array or an empty object for the license list, but found an object with properties.");
				}

				return result;
			default:
				result.AddRange(JsonSerializer.Deserialize<List<License>>(ref reader, options)!);
				return result;
		}
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, LicenseList value, JsonSerializerOptions options) =>
		JsonSerializer.Serialize<List<License>>(writer, value, options);
}
