using System.Reflection;

namespace TheHive.Api.Test.Core;

/// <summary>
/// Keeps the public API free of optional parameters (Sonar S2360): every <see cref="CancellationToken"/> is required, and an operation's
/// optional query, header or multipart parameters travel in one options object.
/// </summary>
public class NoOptionalParametersTests
{
	[Fact]
	public void PublicApi_HasNoOptionalParameters()
	{
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

		var types = typeof(TheHiveClient).Assembly.GetExportedTypes();

		var optional = types
			.SelectMany(t => t.GetMethods(flags).Cast<MethodBase>().Concat(t.GetConstructors(flags))
				.SelectMany(m => m.GetParameters()
					.Where(p => p.IsOptional || p.HasDefaultValue)
					.Select(p => $"{t.FullName}.{m.Name}({p.Name})")))
			.ToList();

		types.Should().NotBeEmpty();
		optional.Should().BeEmpty(
			"the public API takes no optional parameters (pass CancellationToken.None or an empty options object instead), but {0} were found: {1}",
			optional.Count,
			string.Join(", ", optional));
	}
}