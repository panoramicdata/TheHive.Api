using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.AlertStatuses;

/// <summary>A configurable alert status (the spec's <c>OutputAlertStatus</c>).</summary>
public sealed class AlertStatus : StatusEntity<AlertStage>;
