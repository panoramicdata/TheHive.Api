namespace TheHive.Api.Data.AlertFeeders;

/// <summary>
/// The body of an update-alert-feeder request (the spec's <c>InputUpdateAlertFeeder</c>). Unset properties are omitted; the spec defines
/// no clearable fields. The feeder name and function cannot be changed.
/// </summary>
public sealed class AlertFeederUpdateRequest : AlertFeederSettings;
