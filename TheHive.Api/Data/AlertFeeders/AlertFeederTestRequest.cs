namespace TheHive.Api.Data.AlertFeeders;

/// <summary>The body of a test-alert-feeder request (the spec's <c>InputTestAlertFeeder</c>): a feeder configuration to try without saving it. Unset properties are omitted.</summary>
public sealed class AlertFeederTestRequest : AlertFeederNamedSettings;
