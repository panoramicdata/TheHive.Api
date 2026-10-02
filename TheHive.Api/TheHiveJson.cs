using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using TheHive.Api.Converters;
using TheHive.Api.Data.Common;

namespace TheHive.Api;

/// <summary>Shared <see cref="JsonSerializerOptions"/> for TheHive payloads.</summary>
public static class TheHiveJson
{
	/// <summary>The options used by the <see cref="TheHiveClient"/>.</summary>
	public static JsonSerializerOptions Options { get; } = Create();

	private static JsonSerializerOptions Create()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			TypeInfoResolver = new DefaultJsonTypeInfoResolver { Modifiers = { ApplyConventions } }
		};
		options.Converters.Add(new EpochMillisecondsConverter());
		options.Converters.Add(new TolerantEnumConverterFactory());
		options.Converters.Add(new OptionalConverterFactory());
		return options;
	}

	private static void ApplyConventions(JsonTypeInfo typeInfo)
	{
		foreach (var property in typeInfo.Properties)
		{
			// C# 'required' guards construction of requests only; reading responses stays tolerant of missing members.
			property.IsRequired = false;
			if (typeof(IOptional).IsAssignableFrom(property.PropertyType))
			{
				// Unset Optional<T> properties are omitted; a set null is written as JSON null.
				property.ShouldSerialize = static (_, value) => ((IOptional)value!).HasValue;
			}
		}
	}
}
