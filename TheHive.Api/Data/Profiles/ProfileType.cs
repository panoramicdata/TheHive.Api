namespace TheHive.Api.Data.Profiles;

/// <summary>The kind of user a permission profile is for (the spec's <c>ProfileType</c>).</summary>
public enum ProfileType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>For users in non-admin organizations.</summary>
	Organisation,

	/// <summary>For users in the admin organization.</summary>
	Admin,

	/// <summary>For TheHive Portal users (Platinum licence).</summary>
	External
}
