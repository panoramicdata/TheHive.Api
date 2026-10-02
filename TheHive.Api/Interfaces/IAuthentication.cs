using Refit;
using TheHive.Api.Data.Authentication;
using TheHive.Api.Data.Users;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Session login and logout, multifactor (TOTP) set-up and the local password policy (the spec tag <c>Authentication</c>). The client itself always
/// authenticates with its API key (<see cref="TheHiveClientOptions.ApiKey"/>), so most callers never need <see cref="LoginAsync"/>: it is exposed as the plain
/// HTTP operation and does not change how the client authenticates. Passwords, TOTP codes and secrets are sent and returned only in request and response
/// bodies, never in a URL, and this client never logs a body.
/// </summary>
public interface IAuthentication
{
	/// <summary>Gets the password policy enforced by the local authentication provider.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The policy; a rule that is not configured is <see langword="null"/>.</returns>
	[Get("api/v1/auth/local/passwordPolicy")]
	Task<PasswordPolicy> GetPasswordPolicyAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Generates a new TOTP secret and <c>otpauth</c> URI to set up multifactor authentication on your account. Multifactor authentication must not already
	/// be configured. Confirm the secret with <see cref="SetTotpAsync"/>.
	/// </summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The secret and URI. SECRET.</returns>
	[Get("api/v1/auth/totp/get")]
	Task<TotpSecret> GetTotpSecretAsync(CancellationToken cancellationToken = default);

	/// <summary>Activates multifactor authentication by confirming the secret from <see cref="GetTotpSecretAsync"/> with the current authenticator code.</summary>
	/// <param name="request">The code and the secret. SECRET.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/auth/totp/set")]
	Task SetTotpAsync([Body] TotpActivateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deactivates multifactor authentication for your account; not possible when an administrator enforces it for all users.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/auth/totp/unset")]
	Task UnsetTotpAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// Deactivates multifactor authentication for another user. For your own login it behaves like <see cref="UnsetTotpAsync"/>; otherwise it needs
	/// <c>manageUser</c> in an organization that contains the target user.
	/// </summary>
	/// <param name="user">The ID or login of the user.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/auth/totp/unset/{user}")]
	Task UnsetTotpForUserAsync(string user, CancellationToken cancellationToken = default);

	/// <summary>
	/// Logs in with a login, password and organization (and a code when multifactor authentication is enabled). The server answers with a session
	/// cookie (<c>Set-Cookie</c>, not exposed here) and the user profile. After too many failed attempts TheHive temporarily locks the account.
	/// </summary>
	/// <param name="request">The credentials. SECRET.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The signed-in user.</returns>
	[Post("api/v1/login")]
	Task<User> LoginAsync([Body] LoginRequest request, CancellationToken cancellationToken = default);

	/// <summary>Ends the current session and revokes its cookie, using the <c>GET</c> variant that exists for clients that log out through a link or redirect. Prefer <see cref="LogoutAsync"/>.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Get("api/v1/logout")]
	Task LogoutByGetAsync(CancellationToken cancellationToken = default);

	/// <summary>Ends the current session and revokes its cookie (the <c>POST</c> variant, for clients that avoid state-changing GET requests).</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/logout")]
	Task LogoutAsync(CancellationToken cancellationToken = default);
}
