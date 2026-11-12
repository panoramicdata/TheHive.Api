using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Cases;

/// <summary>A TheHive case (the spec's <c>OutputCase</c>).</summary>
public sealed class Case : WorkItemEntity<CaseStage>
{
	/// <summary>The object type name, always <c>Case</c>.</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;

	/// <summary>The incremental case number; usable in place of <see cref="TheHive.Api.Data.Common.TheHiveAuditedEntity.Id"/>.</summary>
	[JsonPropertyName("number")]
	public int Number { get; set; }

	/// <summary>When the incident started.</summary>
	[JsonPropertyName("startDate")]
	public DateTimeOffset StartDate { get; set; }

	/// <summary>When the incident ended, if it has.</summary>
	[JsonPropertyName("endDate")]
	public DateTimeOffset? EndDate { get; set; }

	/// <summary>Whether the case is flagged.</summary>
	[JsonPropertyName("flag")]
	public bool Flag { get; set; }

	/// <summary>The impact verdict; only set when the status is <c>TruePositive</c>.</summary>
	[JsonPropertyName("impactStatus")]
	public ImpactStatus? ImpactStatus { get; set; }

	/// <summary>The access control settings.</summary>
	[JsonPropertyName("access")]
	public Access Access { get; set; } = new() { Kind = AccessKind.Unknown };

	/// <summary>The custom field values.</summary>
	[JsonPropertyName("customFields")]
	public List<CustomFieldValue> CustomFields { get; set; } = [];

	/// <summary>The permissions the current user has on the case.</summary>
	[JsonPropertyName("userPermissions")]
	public List<string> UserPermissions { get; set; } = [];

	/// <summary>When the incident occurred according to the earliest linked alert, if any.</summary>
	[JsonPropertyName("alertDate")]
	public DateTimeOffset? AlertDate { get; set; }

	/// <summary>When the earliest linked alert first entered the <c>New</c> stage, if any.</summary>
	[JsonPropertyName("alertNewDate")]
	public DateTimeOffset? AlertNewDate { get; set; }

	/// <summary>When the earliest linked alert first entered the <c>InProgress</c> stage, if any.</summary>
	[JsonPropertyName("alertInProgressDate")]
	public DateTimeOffset? AlertInProgressDate { get; set; }

	/// <summary>When the earliest linked alert was imported into a case, if any.</summary>
	[JsonPropertyName("alertImportedDate")]
	public DateTimeOffset? AlertImportedDate { get; set; }

	/// <summary>Time to Resolve (TTR), in milliseconds, if known.</summary>
	[JsonPropertyName("timeToResolve")]
	public long? TimeToResolve { get; set; }

	/// <summary>The handling duration, in milliseconds, if known.</summary>
	[JsonPropertyName("handlingDuration")]
	public long? HandlingDuration { get; set; }
}
