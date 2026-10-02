using System.Reflection;
using Refit;

namespace TheHive.Api;

/// <summary>Formats URL parameters like Refit does, except that <see cref="bool"/> values are sent as lowercase <c>true</c>/<c>false</c>, the form the spec's boolean query parameters use.</summary>
internal sealed class TheHiveUrlParameterFormatter : DefaultUrlParameterFormatter
{
	public override string? Format(object? parameterValue, ICustomAttributeProvider attributeProvider, Type type)
		=> parameterValue is bool value
			? value ? "true" : "false"
			: base.Format(parameterValue, attributeProvider, type);
}
