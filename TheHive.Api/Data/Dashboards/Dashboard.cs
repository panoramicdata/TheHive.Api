using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Dashboards;

/// <summary>A dashboard (the spec's <c>OutputDashboard</c>).</summary>
public sealed class Dashboard : TheHiveEntity
{
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
