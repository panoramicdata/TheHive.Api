namespace TheHive.Api.Data.Common;

/// <summary>The access mode of an <see cref="Access"/> (its <c>_kind</c> discriminator).</summary>
public enum AccessKind
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>All external users of the organization can access the entity through TheHive Portal (Platinum licence).</summary>
	AllExternalAccessKind,

	/// <summary>Only the listed external users can access the entity through TheHive Portal (Platinum licence).</summary>
	ExternalAccessKind,

	/// <summary>All internal users of the owner organization can see the entity (the default).</summary>
	OrganisationAccessKind,

	/// <summary>Only the listed internal users can see the entity (Platinum licence).</summary>
	UserAccessKind
}
