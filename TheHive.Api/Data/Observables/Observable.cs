using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Attachments;

namespace TheHive.Api.Data.Observables;

/// <summary>A TheHive observable (the spec's <c>OutputObservable</c>).</summary>
public sealed class Observable
{
	/// <summary>The internal identifier (for example <c>~8529344</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Observable</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the observable.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the observable, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the observable was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the observable was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The observable type (for example <c>ip</c>).</summary>
	[JsonPropertyName("dataType")]
	public string DataType { get; set; } = string.Empty;

	/// <summary>The observable value; <see langword="null"/> for attachment types.</summary>
	[JsonPropertyName("data")]
	public string? Data { get; set; }

	/// <summary>When the observable was created.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset StartDate { get; set; }

	/// <summary>The file, for observable types that require one; <see langword="null"/> for value-based observables.</summary>
	[JsonPropertyName("attachment")]
	public Attachment? Attachment { get; set; }

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int Tlp { get; set; }

	/// <summary>The human-readable TLP label (for example <c>AMBER</c>).</summary>
	[JsonPropertyName("tlpLabel")]
	public string TlpLabel { get; set; } = string.Empty;

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int Pap { get; set; }

	/// <summary>The human-readable PAP label (for example <c>AMBER</c>).</summary>
	[JsonPropertyName("papLabel")]
	public string PapLabel { get; set; } = string.Empty;

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	/// <summary>Whether the observable is an indicator of compromise (IOC).</summary>
	[JsonPropertyName("ioc")]
	public bool Ioc { get; set; }

	/// <summary>Whether the observable has been sighted in the environment.</summary>
	[JsonPropertyName("sighted")]
	public bool Sighted { get; set; }

	/// <summary>When the observable was last sighted, if it has been.</summary>
	[JsonPropertyName("sightedAt")]
	public DateTimeOffset? SightedAt { get; set; }

	/// <summary>The analyzer reports, keyed by analyzer name.</summary>
	[JsonPropertyName("reports")]
	public Dictionary<string, JsonElement> Reports { get; set; } = [];

	/// <summary>The description, if any.</summary>
	[JsonPropertyName("message")]
	public string? Message { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];

	/// <summary>Whether the observable is excluded from similarity checks.</summary>
	[JsonPropertyName("ignoreSimilarity")]
	public bool IgnoreSimilarity { get; set; }

	/// <summary>Whether external users can access the observable through TheHive Portal (Platinum licence).</summary>
	[JsonPropertyName("external")]
	public bool External { get; set; }
}
