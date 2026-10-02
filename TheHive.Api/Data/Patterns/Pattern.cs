using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Patterns;

/// <summary>An attack technique, such as a MITRE ATT&amp;CK technique (the spec's <c>OutputPattern</c>).</summary>
public sealed class Pattern
{
	/// <summary>The internal identifier (for example <c>~163840</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Pattern</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the record.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the record, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the record was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the record was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The MITRE ATT&amp;CK technique identifier (for example <c>T1486</c>).</summary>
	[JsonPropertyName("patternId")]
	public string PatternId { get; set; } = string.Empty;

	/// <summary>The technique name.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The technique description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The tactics the technique belongs to (for example <c>impact</c>).</summary>
	[JsonPropertyName("tactics")]
	public List<string> Tactics { get; set; } = [];

	/// <summary>The URL of the MITRE ATT&amp;CK page for the technique.</summary>
	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;

	/// <summary>The MITRE ATT&amp;CK object type of the entry (for example <c>attack-pattern</c>).</summary>
	[JsonPropertyName("patternType")]
	public string PatternType { get; set; } = string.Empty;

	/// <summary>The CAPEC identifier, if applicable (for example <c>CAPEC-242</c>).</summary>
	[JsonPropertyName("capecId")]
	public string? CapecId { get; set; }

	/// <summary>The URL of the CAPEC entry, if applicable.</summary>
	[JsonPropertyName("capecUrl")]
	public string? CapecUrl { get; set; }

	/// <summary>Whether the technique has been revoked in the MITRE ATT&amp;CK framework.</summary>
	[JsonPropertyName("revoked")]
	public bool Revoked { get; set; }

	/// <summary>The data sources that can detect the technique.</summary>
	[JsonPropertyName("dataSources")]
	public List<string> DataSources { get; set; } = [];

	/// <summary>The defenses the technique can bypass.</summary>
	[JsonPropertyName("defenseBypassed")]
	public List<string> DefenseBypassed { get; set; } = [];

	/// <summary>Guidance for detecting the technique, if any.</summary>
	[JsonPropertyName("detection")]
	public string? Detection { get; set; }

	/// <summary>The permissions required to execute the technique.</summary>
	[JsonPropertyName("permissionsRequired")]
	public List<string> PermissionsRequired { get; set; } = [];

	/// <summary>The target platforms.</summary>
	[JsonPropertyName("platforms")]
	public List<string> Platforms { get; set; } = [];

	/// <summary>Whether the technique can be executed remotely.</summary>
	[JsonPropertyName("remoteSupport")]
	public bool RemoteSupport { get; set; }

	/// <summary>The system requirements for executing the technique.</summary>
	[JsonPropertyName("systemRequirements")]
	public List<string> SystemRequirements { get; set; } = [];

	/// <summary>The version of the technique in the MITRE ATT&amp;CK framework (for example <c>1.3</c>), if any.</summary>
	[JsonPropertyName("version")]
	public string? Version { get; set; }

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
