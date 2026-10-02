using System.Text.Json.Serialization;

namespace TheHive.Api.Data.AuditTrail;

/// <summary>An audit entry with a summary of the entries created by the same request (the spec's <c>OutputStreamAudit</c>).</summary>
public sealed class AuditStreamEntry
{
	/// <summary>The audit entry.</summary>
	[JsonPropertyName("base")]
	public AuditEntry Base { get; set; } = new();

	/// <summary>The count of audit entries created by the same request, grouped by object type and then by action (for example <c>Task</c> then <c>create</c> then <c>2</c>).</summary>
	[JsonPropertyName("summary")]
	public Dictionary<string, Dictionary<string, int>> Summary { get; set; } = [];
}
