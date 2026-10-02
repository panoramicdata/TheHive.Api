using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.UserConfig;

/// <summary>The body of a set-configuration-item request (the spec's <c>InputConfig</c>).</summary>
public sealed class UserConfigSetRequest
{
	/// <summary>The new value to store: any JSON value (an object, array, string, number or boolean). Build it with <c>JsonSerializer.SerializeToElement</c> or <c>JsonDocument.Parse(...).RootElement</c>; an unset (default) element cannot be written.</summary>
	[JsonPropertyName("value")]
	public required JsonElement Value { get; set; }
}
