using System.ComponentModel;
using Refit;
using TheHive.Api.Data.Attachments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Users;

namespace TheHive.Api.Interfaces;

/// <summary>
/// Operations on user accounts: profile, organization memberships, avatar, password and API key. To list users use the query API
/// with <c>listUser</c>. The deprecated <c>DELETE /api/v1/user/{userId}</c> (lock) is not implemented: update with
/// <c>Locked = true</c> instead. Secrets (passwords, API keys) are only ever sent in a request body or returned in a response body;
/// the client never puts them in a URL and never logs them.
/// </summary>
public interface IUsers
{
	/// <summary>Creates a user account (requires <c>manageUser</c>). The avatar cannot be set at creation; use <see cref="UpdateAsync"/>.</summary>
	/// <param name="request">The user to create. Its password is sent in the body only.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The created user.</returns>
	[Post("api/v1/user")]
	Task<User> CreateAsync([Body] UserCreateRequest request, CancellationToken cancellationToken);

	/// <summary>Gets a user account.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The user.</returns>
	[Get("api/v1/user/{userId}")]
	Task<User> GetAsync(string userId, CancellationToken cancellationToken);

	/// <summary>Updates a user account; only the properties set on the request change.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="request">The properties to change.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Patch("api/v1/user/{userId}")]
	Task UpdateAsync(string userId, [Body] UserUpdateRequest request, CancellationToken cancellationToken);

	/// <summary>Gets the account of the authenticated user, including profile, permissions and organization memberships.</summary>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The current user.</returns>
	[Get("api/v1/user/current")]
	Task<User> GetCurrentAsync(CancellationToken cancellationToken);

	/// <summary>Streams the avatar image of a user.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="file">The avatar file name, the last segment of <see cref="User.Avatar"/>.</param>
	/// <param name="options">The <c>If-None-Match</c> header (<see cref="ConditionalDownloadOptions.IfNoneMatch"/>, the <c>ETag</c> of a previous response); pass <c>new()</c> to download unconditionally.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <see cref="TheHiveClientOptions.Timeout"/> bounds only the time until the response headers arrive, not reading the body:
	/// pass a <see cref="CancellationToken"/> to <c>ReadAs*Async</c> (or the stream reads) so a stalled download cannot hang.
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw transport <see cref="GetAvatarWithHeadersAsync"/>; a class implementing <see cref="IUsers"/> only has to provide that method.</para>
	/// </remarks>
	Task<HttpContent> GetAvatarAsync(
		string userId,
		string file,
		ConditionalDownloadOptions options,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(options);
		return GetAvatarWithHeadersAsync(userId, file, options.IfNoneMatch, cancellationToken);
	}

	/// <summary>
	/// The raw transport used by <see cref="GetAvatarAsync"/>, with a <see langword="null"/> <paramref name="ifNoneMatch"/> left out. Call
	/// <see cref="GetAvatarAsync"/> instead; this method exists because Refit cannot turn a property of an object into a request header.
	/// </summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="file">The avatar file name, the last segment of <see cref="User.Avatar"/>.</param>
	/// <param name="ifNoneMatch">The <c>ETag</c> of a previous response, sent as <c>If-None-Match</c>; omitted when <see langword="null"/>.
	/// When it still matches, the server answers 304 and this method throws <see cref="TheHiveApiException"/> with status <c>NotModified</c>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>
	/// The image. Read it with <see cref="HttpContent.ReadAsStreamAsync(CancellationToken)"/>; the <c>ETag</c> is not exposed here.
	/// The caller owns the content and must dispose it.
	/// </returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Get("api/v1/user/{userId}/avatar/{file}")]
	Task<HttpContent> GetAvatarWithHeadersAsync(
		string userId,
		string file,
		[Header("If-None-Match")] string? ifNoneMatch,
		CancellationToken cancellationToken);

	/// <summary>
	/// Permanently deletes a user account (requires <c>manageUser</c>), or removes it from one organization.
	/// Deleting from all organizations (no <see cref="UserDeleteOptions.Organisation"/>) needs the <c>X-Organisation: admin</c> header.
	/// </summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="options">The organization to remove the user from (<see cref="UserDeleteOptions.Organisation"/>, its name or ID); pass <c>new()</c> to delete the user from all organizations.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/user/{userId}/force")]
	Task DeleteAsync(string userId, [Query] UserDeleteOptions options, CancellationToken cancellationToken);

	/// <summary>Replaces the complete set of organizations a user belongs to (requires <c>manageUser</c>); memberships not in the request are removed.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="request">The memberships to apply.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The memberships now in place.</returns>
	[Put("api/v1/user/{userId}/organisations")]
	Task<UserOrganisationsResult> SetOrganisationsAsync(string userId, [Body] UserOrganisationsSetRequest request, CancellationToken cancellationToken);

	/// <summary>Sets the password of a user without needing the current one (requires <c>manageUser</c>).</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="request">The new password; it travels in the request body only. Never log it.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/user/{userId}/password/set")]
	Task SetPasswordAsync(string userId, [Body] UserPasswordSetRequest request, CancellationToken cancellationToken);

	/// <summary>Changes the password of the authenticated user. <paramref name="userId"/> must be the caller's own ID or login.</summary>
	/// <param name="userId">The caller's own user ID preceded by <c>~</c>, or login.</param>
	/// <param name="request">The current and new passwords; they travel in the request body only. Never log them.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Post("api/v1/user/{userId}/password/change")]
	Task ChangePasswordAsync(string userId, [Body] UserPasswordChangeRequest request, CancellationToken cancellationToken);

	/// <summary>Gets the API key of a user. The key is a secret: treat the returned string accordingly and never log it.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The API key, returned by the server as <c>text/plain</c>.</returns>
	[Get("api/v1/user/{userId}/key")]
	Task<string> GetApiKeyAsync(string userId, CancellationToken cancellationToken);

	/// <summary>Revokes the API key of a user; use <see cref="RenewApiKeyAsync"/> to create a new one.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	[Delete("api/v1/user/{userId}/key")]
	Task RevokeApiKeyAsync(string userId, CancellationToken cancellationToken);

	/// <summary>Generates a new API key for a user, invalidating the existing one immediately. The key is a secret: never log it.</summary>
	/// <param name="userId">The user ID preceded by <c>~</c>, or the user login.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The new API key, returned by the server as <c>text/plain</c>.</returns>
	[Post("api/v1/user/{userId}/key/renew")]
	Task<string> RenewApiKeyAsync(string userId, CancellationToken cancellationToken);

	/// <summary>Uploads one or more temporary attachments, which belong to no entity yet and can be referenced when creating or updating a case, alert or other entity.</summary>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "sample.exe", "application/octet-stream")</c>; leave the part name unset.</param>
	/// <param name="options">The <c>canRename</c> form field (<see cref="AttachmentUploadOptions.CanRename"/>: whether the server may rename a file whose name already exists); pass <c>new()</c> to leave it out.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
	/// <remarks>
	/// <para>Uploads are never retried (see <see cref="TheHiveClientOptions.MaxRetries"/>); the per-attempt <see cref="TheHiveClientOptions.Timeout"/> covers sending the files, so raise it for large uploads.</para>
	/// <para>This is the method to call. It is implemented on the interface and sends the request through the raw multipart transport <see cref="UploadTemporaryAttachmentsMultipartAsync"/>; a class implementing <see cref="IUsers"/> only has to provide that method.</para>
	/// </remarks>
	Task<AttachmentUploadResult> UploadTemporaryAttachmentsAsync(
		IEnumerable<MultipartItem> attachments,
		AttachmentUploadOptions options,
		CancellationToken cancellationToken)
	{
		ArgumentNullException.ThrowIfNull(options);
		return UploadTemporaryAttachmentsMultipartAsync(attachments, options.CanRename, cancellationToken);
	}

	/// <summary>
	/// The raw multipart transport used by <see cref="UploadTemporaryAttachmentsAsync"/>, with a <see langword="null"/> <paramref name="canRename"/> left out. Call
	/// <see cref="UploadTemporaryAttachmentsAsync"/> instead; this method exists because Refit cannot turn a property of an object into a multipart form field.
	/// </summary>
	/// <param name="attachments">The files, each sent as a multipart part named <c>attachments</c>. Build each with a file name and,
	/// ideally, a content type, for example <c>new StreamPart(stream, "sample.exe", "application/octet-stream")</c>; leave the part name unset.</param>
	/// <param name="canRename">Whether the server may rename a file whose name already exists; omitted when <see langword="null"/>.</param>
	/// <param name="cancellationToken">A cancellation token.</param>
	/// <returns>The attachments created.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Multipart]
	[Post("api/v1/user/current/attachments")]
	Task<AttachmentUploadResult> UploadTemporaryAttachmentsMultipartAsync(
		[AliasAs("attachments")] IEnumerable<MultipartItem> attachments,
		[AliasAs("canRename")] bool? canRename,
		CancellationToken cancellationToken);
}
