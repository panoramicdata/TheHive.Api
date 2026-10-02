using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Converters;

namespace TheHive.Api.Test.Core;

public class TolerantEnumConverterTests
{
	public enum Colour { Unknown = 0, Red, DarkBlue }

	public enum Rule
	{
		Unknown = 0,
		[JsonStringEnumMemberName("autoShare")] AutoShare,
		[JsonStringEnumMemberName("manual")] Manual
	}

	private static readonly JsonSerializerOptions Options = new() { Converters = { new TolerantEnumConverterFactory() } };

	[Fact]
	public void Read_KnownValue_IsCaseInsensitive() =>
		JsonSerializer.Deserialize<Colour>("\"darkblue\"", Options).Should().Be(Colour.DarkBlue);

	[Fact]
	public void Read_UnknownValue_ReturnsDefault() =>
		JsonSerializer.Deserialize<Colour>("\"Mauve\"", Options).Should().Be(Colour.Unknown);

	[Fact]
	public void Read_NonString_Throws()
	{
		var act = () => JsonSerializer.Deserialize<Colour>("5", Options);
		act.Should().Throw<JsonException>();
	}

	[Fact]
	public void Write_UsesName() =>
		JsonSerializer.Serialize(Colour.Red, Options).Should().Be("\"Red\"");

	[Fact]
	public void Write_UndefinedValue_UsesNumericName() =>
		JsonSerializer.Serialize((Colour)99, Options).Should().Be("\"99\"");

	[Fact]
	public void Write_MemberNameAttribute_UsesWireName() =>
		JsonSerializer.Serialize(Rule.AutoShare, Options).Should().Be("\"autoShare\"");

	[Theory]
	[InlineData("\"autoShare\"", Rule.AutoShare)]
	[InlineData("\"MANUAL\"", Rule.Manual)]
	[InlineData("\"sometimes\"", Rule.Unknown)]
	[InlineData("\"1\"", Rule.Unknown)]
	public void Read_MemberNameAttribute_UsesWireName(string json, Rule expected) =>
		JsonSerializer.Deserialize<Rule>(json, Options).Should().Be(expected);

	[Fact]
	public void Read_Nullable_HandlesNullAndValue()
	{
		JsonSerializer.Deserialize<Colour?>("null", Options).Should().BeNull();
		JsonSerializer.Deserialize<Colour?>("\"red\"", Options).Should().Be(Colour.Red);
	}

	[Fact]
	public void CanConvert_OnlyEnums()
	{
		var factory = new TolerantEnumConverterFactory();
		factory.CanConvert(typeof(Colour)).Should().BeTrue();
		factory.CanConvert(typeof(string)).Should().BeFalse();
	}
}
