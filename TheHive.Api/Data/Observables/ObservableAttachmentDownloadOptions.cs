using Refit;

namespace TheHive.Api.Data.Observables;

/// <summary>
/// The optional query-string flag of <see cref="Interfaces.IObservables.DownloadAttachmentAsync"/>. Pass an empty instance (<c>new()</c>) to download the file itself.
/// </summary>
public sealed class ObservableAttachmentDownloadOptions
{
	/// <summary>
	/// When <see langword="true"/>, the file is wrapped in a password-protected ZIP archive (default password <c>malware</c>) named <c>{name}.zip</c>,
	/// content type <c>application/zip</c>. Sent as <c>asZip</c> (lowercase <c>true</c>/<c>false</c>) and left out when <see langword="null"/>.
	/// Verified against TheHive 5.8: <see langword="false"/> and <see langword="null"/> both return the file itself.
	/// </summary>
	[AliasAs("asZip")]
	public bool? AsZip { get; set; }
}