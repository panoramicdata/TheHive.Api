using Refit;

namespace TheHive.Api.Data.Users;

/// <summary>
/// The optional query-string parameter of <see cref="Interfaces.IUsers.DeleteAsync"/>. Pass an empty instance (<c>new()</c>) to delete the user from all organizations.
/// </summary>
public sealed class UserDeleteOptions
{
	/// <summary>
	/// The name or ID of the organization to remove the user from, sent as <c>organisation</c>; left out when <see langword="null"/>, which deletes the
	/// user from all organizations (that needs the <c>X-Organisation: admin</c> header).
	/// </summary>
	[AliasAs("organisation")]
	public string? Organisation { get; set; }
}