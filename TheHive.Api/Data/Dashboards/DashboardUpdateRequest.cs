using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Dashboards;

/// <summary>The body of an update-dashboard request (the spec's <c>InputUpdateDashboard</c>). Only set properties are sent; the rest keep their values.</summary>
public sealed class DashboardUpdateRequest
{
	/// <summary>The new title (1 to 512 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new group (1 to 32 characters).</summary>
	[JsonPropertyName("group")]
	public string? Group { get; set; }

	/// <summary>The new description.</summary>
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	/// <summary>The new widget layout and configuration, as a JSON object; see <see cref="DashboardCreateRequest.Definition"/>.</summary>
	[JsonPropertyName("definition")]
	public JsonElement? Definition { get; set; }

	/// <summary>The new visibility; <see cref="DashboardStatus.Deleted"/> deletes the dashboard.</summary>
	[JsonPropertyName("status")]
	public DashboardStatus? Status { get; set; }

	/// <summary>The new version of the definition format.</summary>
	[JsonPropertyName("version")]
	public int? Version { get; set; }
}
