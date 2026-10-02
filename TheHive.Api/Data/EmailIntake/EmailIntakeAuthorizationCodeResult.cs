using System.Text.Json.Serialization;

namespace TheHive.Api.Data.EmailIntake;

/// <summary>The result of a Google Workspace authorization-code exchange. The spec declares an untyped response; the description gives <c>{ "id": "&lt;configId&gt;", "authorizationCode": "&lt;code&gt;" }</c>.</summary>
public sealed class EmailIntakeAuthorizationCodeResult
{
	/// <summary>The ID of the new pending configuration; pass it as the <see cref="EmailIntakeConfigInput.Id"/> when adding the configuration.</summary>
	[JsonPropertyName("id")]
	public string Id { get; set; } = string.Empty;

	/// <summary>SECRET. The authorization code echoed back by the server.</summary>
	[JsonPropertyName("authorizationCode")]
	public string AuthorizationCode { get; set; } = string.Empty;
}
