using System.Text.Json;
using TheHive.Api.Data.Common;

namespace TheHive.Api.Test.Core;

public class OptionalTests
{
	public sealed class Probe
	{
		public Optional<string?> Text { get; set; }

		public Optional<DateTimeOffset?> When { get; set; }

		public string? Plain { get; set; }
	}

	public sealed class Strict
	{
		public required string Name { get; set; }
	}

	[Fact]
	public void Unset_HasNoValue()
	{
		var unset = Optional<string?>.Unset;

		unset.HasValue.Should().BeFalse();
		unset.Value.Should().BeNull();
		default(Optional<int>).HasValue.Should().BeFalse();
	}

	[Fact]
	public void Of_Null_HasValueNull()
	{
		var optional = Optional.Of<string?>(null);

		optional.HasValue.Should().BeTrue();
		optional.Value.Should().BeNull();
	}

	[Fact]
	public void ImplicitConversion_SetsValue()
	{
		Optional<int?> optional = 5;

		optional.HasValue.Should().BeTrue();
		optional.Value.Should().Be(5);
	}

	[Fact]
	public void Serialize_Unset_IsOmitted() =>
		JsonSerializer.Serialize(new Probe(), TheHiveJson.Options).Should().Be("{}");

	[Fact]
	public void Serialize_ExplicitNull_WritesNull() =>
		JsonSerializer.Serialize(new Probe { Text = null, When = null }, TheHiveJson.Options)
			.Should().Be("""{"Text":null,"When":null}""");

	[Fact]
	public void Serialize_Value_WritesValueWithConverters() =>
		JsonSerializer.Serialize(new Probe { Text = "x", When = DateTimeOffset.FromUnixTimeMilliseconds(5) }, TheHiveJson.Options)
			.Should().Be("""{"Text":"x","When":5}""");

	[Fact]
	public void Serialize_TopLevelUnset_WritesNull() =>
		JsonSerializer.Serialize(Optional<string?>.Unset, TheHiveJson.Options).Should().Be("null");

	[Fact]
	public void Deserialize_Absent_IsUnset()
	{
		var probe = JsonSerializer.Deserialize<Probe>("{}", TheHiveJson.Options)!;

		probe.Text.HasValue.Should().BeFalse();
		probe.When.HasValue.Should().BeFalse();
	}

	[Fact]
	public void Deserialize_Null_IsSetToNull()
	{
		var probe = JsonSerializer.Deserialize<Probe>("""{"Text":null,"When":null}""", TheHiveJson.Options)!;

		probe.Text.HasValue.Should().BeTrue();
		probe.Text.Value.Should().BeNull();
		probe.When.HasValue.Should().BeTrue();
		probe.When.Value.Should().BeNull();
	}

	[Fact]
	public void RoundTrip_Value_IsPreserved()
	{
		var original = new Probe { Text = "x", When = DateTimeOffset.FromUnixTimeMilliseconds(1700000000000), Plain = "p" };

		var copy = JsonSerializer.Deserialize<Probe>(JsonSerializer.Serialize(original, TheHiveJson.Options), TheHiveJson.Options)!;

		copy.Text.Should().Be(original.Text);
		copy.When.Should().Be(original.When);
		copy.Plain.Should().Be("p");
	}

	[Fact]
	public void Deserialize_MissingRequiredMember_IsTolerated() =>
		JsonSerializer.Deserialize<Strict>("{}", TheHiveJson.Options)!.Name.Should().BeNull();
}
