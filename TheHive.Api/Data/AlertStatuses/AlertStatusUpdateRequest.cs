using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.AlertStatuses;

/// <summary>The body of an update-alert-status request (the spec's <c>InputUpdateAlertStatus</c>). Only set properties are sent; set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field. <c>value</c> and <c>stage</c> cannot be changed.</summary>
public sealed class AlertStatusUpdateRequest
{
	/// <summary>The new position in the UI dropdown list; set to <see langword="null"/> to remove the custom ordering.</summary>
	[JsonPropertyName("order")]
	public Optional<int?> Order { get; set; }

	/// <summary>The new description (up to 1048576 characters); set to <see langword="null"/> to remove it.</summary>
	[JsonPropertyName("description")]
	public Optional<string?> Description { get; set; }

	/// <summary>The new hex colour (for example <c>#52c41a</c>); set to <see langword="null"/> to remove the custom colour.</summary>
	[JsonPropertyName("colour")]
	public Optional<string?> Colour { get; set; }

	/// <summary>Whether to hide the status from the UI dropdown.</summary>
	[JsonPropertyName("hidden")]
	public bool? Hidden { get; set; }
}
