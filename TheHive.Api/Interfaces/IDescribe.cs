using Refit;
using TheHive.Api.Data.Describe;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Entity model metadata: each model's properties, their type, cardinality and index type, and whether they can be filtered,
/// sorted or aggregated in a query (<c>POST /api/v1/query</c>).
/// </summary>
public interface IDescribe
{
	/// <summary>Describes every entity model at once.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The descriptions keyed by model name, for example <c>case</c>.</returns>
	[Get("api/v1/describe/_all")]
	Task<Dictionary<string, EntityDescription>> GetAllAsync(CancellationToken cancellationToken = default);

	/// <summary>Describes one entity model.</summary>
	/// <param name="model">The model name, for example <c>case</c>, <c>alert</c>, <c>task</c>, <c>observable</c> or <c>user</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The model's description.</returns>
	[Get("api/v1/describe/{model}")]
	Task<EntityDescription> GetAsync(string model, CancellationToken cancellationToken = default);
}
