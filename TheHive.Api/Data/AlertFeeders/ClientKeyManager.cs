using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>Key manager or trust manager settings (the spec's <c>Manager</c>).</summary>
public sealed class ClientKeyManager
{
	/// <summary>The algorithm name of the manager (1 to 128 characters).</summary>
	[JsonPropertyName("algorithm")]
	public string? Algorithm { get; set; }

	/// <summary>The key or trust stores the manager uses.</summary>
	[JsonPropertyName("stores")]
	public List<ClientKeyStore>? Stores { get; set; }
}
