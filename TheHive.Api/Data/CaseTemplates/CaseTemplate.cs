using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.CaseTemplates;

/// <summary>A case template (the spec's <c>OutputCaseTemplate</c>).</summary>
public sealed class CaseTemplate
{
	/// <summary>The internal identifier (for example <c>~84123</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>caseTemplate</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the template.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the template, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the template was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the template was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The unique name of the template within the organization.</summary>
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>The name shown to users when selecting the template.</summary>
	[JsonPropertyName("displayName")]
	public string DisplayName { get; set; } = string.Empty;

	/// <summary>The prefix added to case titles when the template is applied, if any.</summary>
	[JsonPropertyName("titlePrefix")]
	public string? TitlePrefix { get; set; }

	/// <summary>The default case description, if any.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The default severity, 1 (low) to 4 (critical), if set; see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public int? Severity { get; set; }

	/// <summary>The human-readable severity label (for example <c>HIGH</c>), if a severity is set.</summary>
	[JsonPropertyName("severityLabel")]
	public string? SeverityLabel { get; set; }

	/// <summary>The default tags.</summary>
	[JsonPropertyName("tags")]
	public List<string> Tags { get; set; } = [];

	/// <summary>Whether cases are flagged as important by default.</summary>
	[JsonPropertyName("flag")]
	public bool Flag { get; set; }

	/// <summary>The default TLP level, if set; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public int? Tlp { get; set; }

	/// <summary>The human-readable TLP label (for example <c>AMBER</c>), if a TLP is set.</summary>
	[JsonPropertyName("tlpLabel")]
	public string? TlpLabel { get; set; }

	/// <summary>The default PAP level, if set; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public int? Pap { get; set; }

	/// <summary>The human-readable PAP label (for example <c>AMBER</c>), if a PAP is set.</summary>
	[JsonPropertyName("papLabel")]
	public string? PapLabel { get; set; }

	/// <summary>The default case summary, if any.</summary>
	[JsonPropertyName("summary")]
	public string? Summary { get; set; }

	/// <summary>The default custom field values.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldValue> CustomFields { get; set; } = [];

	/// <summary>The task templates created on cases when the template is applied.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTask> Tasks { get; set; } = [];

	/// <summary>Extra data fields, populated when specific keys are requested.</summary>
	[JsonPropertyName("extraData")]
	public Dictionary<string, JsonElement> ExtraData { get; set; } = [];
}
