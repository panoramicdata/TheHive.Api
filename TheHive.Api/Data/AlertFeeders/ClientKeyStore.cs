using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AlertFeeders;

/// <summary>A key or trust store used by a <see cref="ClientKeyManager"/> (the spec's <c>StoreConfig</c>).</summary>
public sealed class ClientKeyStore
{
	/// <summary>SECRET when it holds private keys. The PEM or base64-encoded store data, used instead of <see cref="FilePath"/>.</summary>
	[JsonPropertyName("data")]
	public string? Data { get; set; }

	/// <summary>The path of the store file on disk.</summary>
	[JsonPropertyName("filePath")]
	public string? FilePath { get; set; }

	/// <summary>Whether the store file is on the classpath.</summary>
	[JsonPropertyName("isFileOnClasspath")]
	public bool? IsFileOnClasspath { get; set; }

	/// <summary>SECRET. The password that unlocks the store file; only ever in a request or response body.</summary>
	[JsonPropertyName("password")]
	public string? Password { get; set; }

	/// <summary>The store format. The server requires it.</summary>
	[JsonPropertyName("type")]
	public string? Type { get; set; }
}
