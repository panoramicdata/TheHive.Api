using System.Reflection;

namespace TheHive.Api.Test.Core;

/// <summary>Keeps the group interfaces implementable outside TheHive.Api (by callers' own classes and by mocking libraries).</summary>
public class InterfaceSurfaceTests
{
	[Fact]
	public void GroupInterfaces_HaveNoNonPublicMembers()
	{
		var interfaces = typeof(TheHiveClient).Assembly.GetExportedTypes()
			.Where(t => t.IsInterface && t.Namespace == "TheHive.Api.Interfaces")
			.ToList();

		var nonPublic = interfaces
			.SelectMany(t => t.GetMembers(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
				.Select(m => $"{t.Name}.{m.Name}"))
			.ToList();

		interfaces.Should().NotBeEmpty();
		nonPublic.Should().BeEmpty("a non-public interface member cannot be implemented by an assembly without InternalsVisibleTo, so callers could not implement or mock the interface");
	}
}
