namespace TheHive.Api.Data.Dashboards;

/// <summary>The visibility of a dashboard (the spec's <c>DashboardStatus</c>).</summary>
public enum DashboardStatus
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Only the owner can see the dashboard.</summary>
	Private,

	/// <summary>The whole organization can see the dashboard.</summary>
	Shared,

	/// <summary>The dashboard is deleted; setting it on an update deletes the dashboard.</summary>
	Deleted
}
