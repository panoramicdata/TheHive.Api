using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Licenses;

/// <summary>
/// The licenses stored in TheHive. The spec's <c>List_OutputLicense</c> is a <c>oneOf</c> of the license array and an empty object (<c>Nil</c>);
/// this list reads either, so an empty object becomes an empty list instead of failing.
/// </summary>
[JsonConverter(typeof(LicenseListConverter))]
public sealed class LicenseList : List<License>
{
}
