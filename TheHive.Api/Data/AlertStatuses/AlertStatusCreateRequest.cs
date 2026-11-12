using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.AlertStatuses;

/// <summary>The body of a create-alert-status request (the spec's <c>InputCreateAlertStatus</c>). Unset properties are omitted. <c>value</c> and <c>stage</c> cannot be changed after creation.</summary>
public sealed class AlertStatusCreateRequest : StatusCreateRequestBase<AlertStage>;
