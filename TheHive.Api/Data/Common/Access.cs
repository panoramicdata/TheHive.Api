using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Common;

/// <summary>Access control settings (the spec's polymorphic <c>Access</c> schema, flattened).</summary>
public sealed class Access
{
	/// <summary>The access mode.</summary>
	[JsonPropertyName("_kind")]
	public AccessKind Kind { get; set; }

	/// <summary>The user logins (email addresses) granted access; only used by <see cref="AccessKind.ExternalAccessKind"/> and <see cref="AccessKind.UserAccessKind"/>.</summary>
	[JsonPropertyName("users")]
	public List<string>? Users { get; set; }
}
