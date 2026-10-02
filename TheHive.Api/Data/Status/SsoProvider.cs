using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Status;

/// <summary>A single sign-on provider offered on the sign-in page (the spec's <c>OutputSsoProvider</c>).</summary>
public sealed class SsoProvider
{
	/// <summary>The internal identifier of the provider (for example <c>okta</c>).</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The label shown on the provider's sign-in button.</summary>
	[JsonPropertyName("displayName")]
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>The URL that starts the sign-in flow for the provider.</summary>
	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;
}
