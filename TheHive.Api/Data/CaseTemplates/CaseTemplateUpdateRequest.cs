using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Data.CaseTemplates;

/// <summary>The body of an update-case-template request (the spec's <c>InputUpdateCaseTemplate</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
public sealed class CaseTemplateUpdateRequest
{
	/// <summary>The new name (1 to 128 characters); unique in the organization.</summary>
	[JsonPropertyName("name")]
	public string? Name { get; set; }

	/// <summary>The new display name shown to users when selecting the template.</summary>
	[JsonPropertyName("displayName")]
	public string? DisplayName { get; set; }

	/// <summary>The new title prefix; set to <see langword="null"/> to remove it.</summary>
	[JsonPropertyName("titlePrefix")]
	public Optional<string?> TitlePrefix { get; set; }

	/// <summary>The new default case description; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("description")]
	public Optional<string?> Description { get; set; }

	/// <summary>The new default severity, 1 (low) to 4 (critical); set to <see langword="null"/> to clear it. See <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	public Optional<int?> Severity { get; set; }

	/// <summary>The tags that replace the current tag list; unknown tags are created.</summary>
	[JsonPropertyName("tags")]
	public List<string>? Tags { get; set; }

	/// <summary>Whether cases are flagged as important by default.</summary>
	[JsonPropertyName("flag")]
	public bool? Flag { get; set; }

	/// <summary>The new default TLP level, 0 to 4; set to <see langword="null"/> to clear it. See <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	public Optional<int?> Tlp { get; set; }

	/// <summary>The new default PAP level, 0 to 3; set to <see langword="null"/> to clear it. See <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	public Optional<int?> Pap { get; set; }

	/// <summary>The new default case summary; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("summary")]
	public Optional<string?> Summary { get; set; }

	/// <summary>The custom field values; in this array form each listed custom field's values are replaced and custom fields not listed are deleted.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldInput>? CustomFields { get; set; }

	/// <summary>The task templates that replace the current task list.</summary>
	[JsonPropertyName("tasks")]
	public List<CaseTaskCreateRequest>? Tasks { get; set; }
}
