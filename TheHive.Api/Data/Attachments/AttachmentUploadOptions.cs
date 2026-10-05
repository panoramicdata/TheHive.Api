namespace TheHive.Api.Data.Attachments;

/// <summary>
/// The optional form field of an attachment upload (to a case, an alert, a case report template, the organization, or the current user's temporary
/// attachments). Pass an empty instance (<c>new()</c>) to leave it out.
/// </summary>
public sealed class AttachmentUploadOptions
{
	/// <summary>
	/// Whether the server may rename a file whose name already exists, sent as the <c>canRename</c> form field; left out when <see langword="null"/>.
	/// </summary>
	public bool? CanRename { get; set; }
}