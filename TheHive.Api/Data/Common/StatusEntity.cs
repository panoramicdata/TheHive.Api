using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>A configurable status (the shared shape of case and alert statuses).</summary>
/// <typeparam name="TStage">The stage enumeration the status belongs to.</typeparam>
public abstract class StatusEntity<TStage> : TheHiveEntity
	where TStage : struct, Enum
{
	/// <summary>The display name of the status; it is what an object's status holds.</summary>
	[JsonPropertyName("value")]
	public string Value { get; set; } = string.Empty;

	/// <summary>The stage the status belongs to.</summary>
	[JsonPropertyName("stage")]
	public TStage Stage { get; set; }

	/// <summary>The position of the status in the UI dropdown list, if set.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>A human-readable description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour used to display the status in the UI, if set.</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }

	/// <summary>Whether the status is hidden from the UI dropdown; hidden statuses stay usable through the API.</summary>
	[JsonPropertyName("hidden")]
	public bool Hidden { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
