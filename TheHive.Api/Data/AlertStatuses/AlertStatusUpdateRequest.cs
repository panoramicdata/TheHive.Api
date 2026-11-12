using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.AlertStatuses;

/// <summary>The body of an update-alert-status request (the spec's <c>InputUpdateAlertStatus</c>). Only set properties are sent; set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field. <c>value</c> and <c>stage</c> cannot be changed.</summary>
public sealed class AlertStatusUpdateRequest : StatusUpdateRequestBase;
