using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Dashboards;

/// <summary>A dashboard (the spec's <c>OutputDashboard</c>).</summary>
public sealed class Dashboard
{
	/// <summary>The internal identifier (for example <c>~123456</c>).</summary>
	[JsonPropertyName("_id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>The entity type, always <c>Dashboard</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The login of the user who created the dashboard.</summary>
	[JsonPropertyName("_createdBy")]
	public string CreatedBy { get; set; } = string.Empty;

	/// <summary>The login of the user who last updated the dashboard, if it has been updated.</summary>
	[JsonPropertyName("_updatedBy")]
	public string? UpdatedBy { get; set; }

	/// <summary>When the dashboard was created.</summary>
	[JsonPropertyName("_createdAt")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>When the dashboard was last updated, if it has been updated.</summary>
	[JsonPropertyName("_updatedAt")]
	public DateTimeOffset? UpdatedAt { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")]
	public string Title { get; set; } = string.Empty;

	/// <summary>The group used to categorize the dashboard on the dashboards list.</summary>
	[JsonPropertyName("group")]
	public string Group { get; set; } = string.Empty;

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	/// <summary>The visibility.</summary>
	[JsonPropertyName("status")]
	public DashboardStatus Status { get; set; }

	/// <summary>The login of the owner, if any.</summary>
	[JsonPropertyName("owner")]
	public string? Owner { get; set; }

	/// <summary>
	/// The widget layout and configuration, a JSON object structured as rows of widgets. The spec calls it an object, not a string,
	/// and does not document its structure, so it is kept as raw JSON; <c>ValueKind</c> is <c>Undefined</c> if a response omits it.
	/// </summary>
	[JsonPropertyName("definition")]
	public JsonElement Definition { get; set; }

	/// <summary>Whether the current user can update or delete the dashboard.</summary>
	[JsonPropertyName("writable")]
	public bool Writable { get; set; }

	/// <summary>The version of the definition format; the current format is 1.</summary>
	[JsonPropertyName("version")]
	public int Version { get; set; }
}
