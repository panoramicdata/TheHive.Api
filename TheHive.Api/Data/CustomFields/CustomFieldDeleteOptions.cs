using Refit;

namespace TheHive.Api.Data.CustomFields;

/// <summary>
/// The optional query-string flag of <see cref="Interfaces.ICustomFields.DeleteAsync"/>. Pass an empty instance (<c>new()</c>) to leave it out.
/// </summary>
public sealed class CustomFieldDeleteOptions
{
	/// <summary>
	/// Whether to delete the field even when cases or alerts use it, sent as <c>force</c> (lowercase <c>true</c>/<c>false</c>); left out when
	/// <see langword="null"/> (the server then refuses to delete a field in use).
	/// </summary>
	[AliasAs("force")]
	public bool? Force { get; set; }
}