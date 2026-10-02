using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Procedures;

/// <summary>The body of an add-several-procedures request (the spec's <c>InputBulkProcedure</c>).</summary>
public sealed class ProcedureBulkCreateRequest
{
	/// <summary>The procedures to add.</summary>
	[JsonPropertyName("procedures")]
	public List<ProcedureInput> Procedures { get; set; } = [];
}
