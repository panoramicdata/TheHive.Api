using Refit;
using TheHive.Api.Data.EmailIntake;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on the email intake module, which turns emails from connected mailboxes into alerts (Gold or Platinum licence; OAuth 2.0
/// providers need Platinum; all need <c>manageConfig</c>, with the <c>X-Organisation: admin</c> header to target the admin organization).
/// Mailbox configurations hold credentials (passwords, OAuth 2.0 secrets, authorization codes): they only ever travel in request and response
/// bodies, never in a URL.
/// </summary>
public interface IEmailIntake
{
	/// <summary>Lists the email intake providers available on the server.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The providers, with the name and kind to use when configuring a mailbox.</returns>
	[Get("api/v1/connector/email-intake/providers")]
	Task<List<EmailIntakeProviderInfo>> ListProvidersAsync(CancellationToken cancellationToken = default);

	/// <summary>Adds a mailbox configuration. Test it first with <see cref="TestConfigAsync"/>.</summary>
	/// <param name="request">The mailbox configuration, including credentials.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created configuration.</returns>
	[Post("api/v1/connector/email-intake/config")]
	Task<EmailIntakeConfig> CreateConfigAsync([Body] EmailIntakeConfigInput request, CancellationToken cancellationToken = default);

	/// <summary>Permanently removes a mailbox configuration.</summary>
	/// <param name="configId">The configuration ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/connector/email-intake/config/{configId}")]
	Task DeleteConfigAsync(string configId, CancellationToken cancellationToken = default);

	/// <summary>Gets a mailbox configuration.</summary>
	/// <param name="configId">The configuration ID preceded by <c>~</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The configuration.</returns>
	[Get("api/v1/connector/email-intake/config/{configId}")]
	Task<EmailIntakeConfig> GetConfigAsync(string configId, CancellationToken cancellationToken = default);

	/// <summary>Replaces a mailbox configuration.</summary>
	/// <param name="configId">The configuration ID preceded by <c>~</c>.</param>
	/// <param name="request">The new configuration, including credentials.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/connector/email-intake/config/{configId}")]
	Task UpdateConfigAsync(string configId, [Body] EmailIntakeConfigInput request, CancellationToken cancellationToken = default);

	/// <summary>Exchanges a Google Workspace OAuth 2.0 authorization code for a new pending configuration entry (Platinum licence). Pass the returned ID as <see cref="EmailIntakeConfigInput.Id"/> when calling <see cref="CreateConfigAsync"/>.</summary>
	/// <param name="request">The authorization code obtained from the Google consent flow.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new configuration ID.</returns>
	[Post("api/v1/connector/email-intake/config/authorizationCode")]
	Task<EmailIntakeAuthorizationCodeResult> SetAuthorizationCodeAsync([Body] EmailIntakeAuthorizationCodeRequest request, CancellationToken cancellationToken = default);

	/// <summary>Tests the connection to a mailbox without saving the configuration. An unreachable mailbox or wrong credentials answer with an error status.</summary>
	/// <param name="request">The mailbox configuration, including credentials.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/connector/email-intake/config/test")]
	Task TestConfigAsync([Body] EmailIntakeConfigInput request, CancellationToken cancellationToken = default);

	/// <summary>Gets the email intake module configuration: whether it is enabled, the sync interval and every mailbox configuration.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The module configuration.</returns>
	[Get("api/v1/connector/email-intake/configs")]
	Task<EmailIntakeConfigsResult> GetConfigsAsync(CancellationToken cancellationToken = default);

	/// <summary>Updates the module-wide settings (enabled flag and sync interval); mailbox configurations are managed with the single-configuration operations.</summary>
	/// <param name="request">The new settings.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Put("api/v1/connector/email-intake/configs")]
	Task UpdateConfigsAsync([Body] EmailIntakeModuleUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Lists the folders of the mailbox described by the request, to choose <c>mailbox.inbox</c> and <c>mailbox.archive</c>.</summary>
	/// <param name="request">The mailbox configuration, including credentials.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The folder names.</returns>
	[Post("api/v1/connector/email-intake/folders")]
	Task<List<string>> ListFoldersAsync([Body] EmailIntakeConfigInput request, CancellationToken cancellationToken = default);

	/// <summary>Triggers an immediate email fetch without waiting for the next interval.</summary>
	/// <param name="options">The mailbox configuration to sync (<see cref="EmailIntakeSyncOptions.ConfigId"/>); pass <c>new()</c> to sync every connected mailbox. The spec describes this <c>configId</c> query parameter only in prose, not as a declared parameter.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/connector/email-intake/sync")]
	Task SyncAsync([Query] EmailIntakeSyncOptions options, CancellationToken cancellationToken = default);
}
