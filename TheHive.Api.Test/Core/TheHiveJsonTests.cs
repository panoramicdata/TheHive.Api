using System.Text.Json;

namespace TheHive.Api.Test.Core;

public class TheHiveJsonTests
{
	public enum Shade { Unknown = 0, Red }

	public sealed record Sample(DateTimeOffset X, Shade Y, string? Z = null);

	[Fact]
	public void Deserialize_UsesEpochAndTolerantEnumConverters()
	{
		var sample = JsonSerializer.Deserialize<Sample>("{\"x\":1700000000000,\"y\":\"Red\"}", TheHiveJson.Options);

		sample.Should().NotBeNull();
		sample!.X.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000000000));
		sample.Y.Should().Be(Shade.Red);
	}

	[Fact]
	public void Serialize_OmitsNullProperties()
	{
		var json = JsonSerializer.Serialize(new Sample(DateTimeOffset.FromUnixTimeMilliseconds(1700000000000), Shade.Red), TheHiveJson.Options);

		json.Should().NotContain("Z").And.NotContain("null");
	}

	[Fact]
	public void Options_AreReadOnly_SoMutationThrows()
	{
		TheHiveJson.Options.IsReadOnly.Should().BeTrue();
		var act = () => TheHiveJson.Options.PropertyNameCaseInsensitive = false;
		act.Should().Throw<InvalidOperationException>();
	}
}
