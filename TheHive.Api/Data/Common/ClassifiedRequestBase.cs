using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>The severity, flag, protocol and tag properties shared by case, case-template and alert request bodies.</summary>
public abstract class ClassifiedRequestBase
{
	/// <summary>The severity, 1 (low) to 4 (critical); see <see cref="Common.Severity"/>.</summary>
	[JsonPropertyName("severity")]
	[JsonPropertyOrder(100)]
	public int? Severity { get; set; }

	/// <summary>The tags; on create unknown tags are created, on update they replace the current set.</summary>
	[JsonPropertyName("tags")]
	[JsonPropertyOrder(200)]
	public List<string>? Tags { get; set; }

	/// <summary>Whether to flag the object.</summary>
	[JsonPropertyName("flag")]
	[JsonPropertyOrder(300)]
	public bool? Flag { get; set; }

	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	[JsonPropertyOrder(400)]
	public int? Tlp { get; set; }

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	[JsonPropertyOrder(500)]
	public int? Pap { get; set; }
}
