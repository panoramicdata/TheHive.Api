using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>A stored object that also reports its type name as <c>_type</c>.</summary>
public abstract class TheHiveEntity : TheHiveAuditedEntity
{
	/// <summary>The object type name (for example <c>Case</c>).</summary>
	[JsonPropertyName("_type")]
	public string Type { get; set; } = string.Empty;
}
