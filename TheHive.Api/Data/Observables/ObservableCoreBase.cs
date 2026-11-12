using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Observables;

/// <summary>The protocol, tag and indicator properties shared by the observable create and update request bodies.</summary>
public abstract class ObservableCoreBase
{
	/// <summary>The Traffic Light Protocol level, 0 to 4; see <see cref="Common.Tlp"/>.</summary>
	[JsonPropertyName("tlp")]
	[JsonPropertyOrder(100)]
	public int? Tlp { get; set; }

	/// <summary>The Permissible Actions Protocol level, 0 to 3; see <see cref="Common.Pap"/>.</summary>
	[JsonPropertyName("pap")]
	[JsonPropertyOrder(200)]
	public int? Pap { get; set; }

	/// <summary>The tags; on update they replace the current set.</summary>
	[JsonPropertyName("tags")]
	[JsonPropertyOrder(300)]
	public List<string>? Tags { get; set; }

	/// <summary>Whether the observable is an indicator of compromise.</summary>
	[JsonPropertyName("ioc")]
	[JsonPropertyOrder(400)]
	public bool? Ioc { get; set; }

	/// <summary>Whether the observable has been sighted.</summary>
	[JsonPropertyName("sighted")]
	[JsonPropertyOrder(500)]
	public bool? Sighted { get; set; }
}
