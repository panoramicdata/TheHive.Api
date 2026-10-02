using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>Reads a <see cref="LicenseList"/> from a JSON array or from an object (the spec's <c>Nil</c>, read as no licenses).</summary>
internal sealed class LicenseListConverter : JsonConverter<LicenseList>
{
	/// <inheritdoc />
	public override LicenseList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var result = new LicenseList();
		if (reader.TokenType == JsonTokenType.StartObject)
		{
			reader.Skip();
			return result;
		}

		result.AddRange(JsonSerializer.Deserialize<List<License>>(ref reader, options)!);
		return result;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, LicenseList value, JsonSerializerOptions options) =>
		JsonSerializer.Serialize<List<License>>(writer, value, options);
}
