using System.Text.Json.Serialization;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Data.Timeline;

/// <summary>The body of an update-custom-event request (the spec's <c>InputUpdateCustomEvent</c>). Only set properties are sent; the rest keep their values. Set an <see cref="Optional{T}"/> property to <see langword="null"/> to clear that field.</summary>
public sealed class CustomEventUpdateRequest
{
	/// <summary>The new start date.</summary>
	[JsonPropertyName("date")]
	public DateTimeOffset? Date { get; set; }

	/// <summary>The new end date; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("endDate")]
	public Optional<DateTimeOffset?> EndDate { get; set; }

	/// <summary>The new title (1 to 128 characters).</summary>
	[JsonPropertyName("title")]
	public string? Title { get; set; }

	/// <summary>The new description; set to <see langword="null"/> to clear it.</summary>
	[JsonPropertyName("description")]
	public Optional<string?> Description { get; set; }
}
