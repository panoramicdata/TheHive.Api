using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Dashboards;

/// <summary>The body of a create-dashboard request (the spec's <c>InputCreateDashboard</c>). Unset properties are omitted.</summary>
public sealed class DashboardCreateRequest
{
	/// <summary>The title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public required string Title { get; set; }

	/// <summary>The group used to categorize the dashboard on the dashboards list (1 to 32 characters); the server default is <c>default</c>.</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The description.</summary>
	[JsonPropertyName("description")]
	public required string Description { get; set; }

	/// <summary>The visibility; <see cref="DashboardStatus.Deleted"/> has no effect on creation and behaves like <see cref="DashboardStatus.Private"/>.</summary>
	[JsonPropertyName("status")]
	public required DashboardStatus Status { get; set; }

	/// <summary>
	/// The widget layout and configuration, a JSON object structured as rows of widgets. The structure is not documented by the spec;
	/// the simplest way to build a valid value is to create a dashboard in the TheHive web interface, read it back with
	/// <c>IDashboards.GetAsync</c> and start from its <c>Definition</c>.
	/// </summary>
	/// <remarks>A default (<c>Undefined</c>) <see cref="JsonElement"/> throws <see cref="InvalidOperationException"/> when serialised: supply a real JSON object, for example <c>JsonDocument.Parse(json).RootElement</c>.</remarks>
	[JsonPropertyName("definition")]
	public required JsonElement Definition { get; set; }

	/// <summary>The version of the definition format; the server default, and current format, is 1.</summary>
	[JsonPropertyName("version")]
	public int? Version { get; set; }
}
