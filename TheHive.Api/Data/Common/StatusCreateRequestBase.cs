using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>The body shared by create-status requests. Unset properties are omitted; <c>value</c> and <c>stage</c> cannot be changed after creation.</summary>
/// <typeparam name="TStage">The stage enumeration the status belongs to.</typeparam>
public abstract class StatusCreateRequestBase<TStage>
	where TStage : struct, Enum
{
	/// <summary>The display name of the status (1 to 64 characters); immutable after creation.</summary>
	[JsonPropertyName("value")]
	public required string Value { get; set; }

	/// <summary>The stage the status belongs to; immutable after creation.</summary>
	[JsonPropertyName("stage")]
	public required TStage Stage { get; set; }

	/// <summary>The position of the status in the UI dropdown list; lower values appear first.</summary>
	[JsonPropertyName("order")]
	public int? Order { get; set; }

	/// <summary>A human-readable description (up to 1048576 characters).</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The hex colour used to display the status in the UI.</summary>
	[JsonPropertyName("colour")]
	public string? Colour { get; set; }

	/// <summary>Whether to hide the status from the UI dropdown; hidden statuses stay usable through the API.</summary>
	[JsonPropertyName("hidden")]
	public bool? Hidden { get; set; }
}
