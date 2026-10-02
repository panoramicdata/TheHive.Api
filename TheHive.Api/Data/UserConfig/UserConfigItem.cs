using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.UserConfig;

/// <summary>A user configuration item (the spec's <c>OutputConfig</c>). Its values can be any JSON value, so they stay raw JSON.</summary>
public sealed class UserConfigItem
{
	/// <summary>The key of the configuration item (for example <c>dashboards</c>).</summary>
	[JsonPropertyName("path")]
	public string Path { get; set; } = string.Empty;

	/// <summary>The default value, returned when no override is set. Its <see cref="JsonElement.ValueKind"/> is <see cref="JsonValueKind.Undefined"/> if the server left it out.</summary>
	[JsonPropertyName("defaultValue")]
	public JsonElement DefaultValue { get; set; }

	/// <summary>The current value: the override if one is set, otherwise the default. Its <see cref="JsonElement.ValueKind"/> is <see cref="JsonValueKind.Undefined"/> if the server left it out.</summary>
	[JsonPropertyName("value")]
	public JsonElement Value { get; set; }
}
