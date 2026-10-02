using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Converters;

namespace TheHive.Api;

/// <summary>Shared <see cref="JsonSerializerOptions"/> for TheHive payloads.</summary>
public static class TheHiveJson
{
	/// <summary>The options used by the TheHive client.</summary>
	public static JsonSerializerOptions Options { get; } = Create();

	private static JsonSerializerOptions Create()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		options.Converters.Add(new EpochMillisecondsConverter());
		options.Converters.Add(new TolerantEnumConverterFactory());
		return options;
	}
}
