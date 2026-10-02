using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Authentication;

/// <summary>
/// The password policy enforced by the local authentication provider. The spec types the response as an untyped object and documents these members
/// (verify: the names come from its description and example). A rule that is not configured is absent, so it is <see langword="null"/>.
/// </summary>
public sealed class PasswordPolicy
{
	/// <summary>Whether a password policy is enforced.</summary>
	[JsonPropertyName("enabled")]
	public bool Enabled { get; set; }

	/// <summary>The minimum number of characters, if a rule is configured.</summary>
	[JsonPropertyName("minLength")]
	public int? MinLength { get; set; }

	/// <summary>The minimum number of lowercase characters, if a rule is configured.</summary>
	[JsonPropertyName("minLowerCase")]
	public int? MinLowerCase { get; set; }

	/// <summary>The minimum number of uppercase characters, if a rule is configured.</summary>
	[JsonPropertyName("minUpperCase")]
	public int? MinUpperCase { get; set; }

	/// <summary>The minimum number of digits, if a rule is configured.</summary>
	[JsonPropertyName("minDigit")]
	public int? MinDigit { get; set; }

	/// <summary>The minimum number of special characters, if a rule is configured.</summary>
	[JsonPropertyName("minSpecial")]
	public int? MinSpecial { get; set; }

	/// <summary>Whether a password cannot contain the user's login, if a rule is configured.</summary>
	[JsonPropertyName("cannotContainUsername")]
	public bool? CannotContainUsername { get; set; }
}
