namespace TheHive.Api.Data.Users;

/// <summary>The type of a user account (the spec's <c>UserType</c>).</summary>
public enum UserType
{
	/// <summary>A value this client does not recognise.</summary>
	Unknown = 0,

	/// <summary>Authenticates with an API key only; cannot sign in to the web interface.</summary>
	Service,

	/// <summary>Can use the web interface and the API.</summary>
	Normal,

	/// <summary>Accesses TheHive through the Portal (requires a Platinum licence). The type cannot be changed to or from this value after creation.</summary>
	External
}
