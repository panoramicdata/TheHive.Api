using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Dashboards;

/// <summary>The body of a change-dashboard-owner request (the spec's <c>InputChangeDashboardOwnership</c>).</summary>
public sealed class DashboardOwnerChangeRequest
{
	/// <summary>The ID or login of the user in your organization who becomes the owner.</summary>
	[JsonPropertyName("user")]
	public required string User { get; set; }
}
