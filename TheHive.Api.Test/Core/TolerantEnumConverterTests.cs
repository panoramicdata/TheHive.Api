using System.Text.Json;
using TheHive.Api.Converters;

namespace TheHive.Api.Test.Core;

public class TolerantEnumConverterTests
{
	public enum Colour { Unknown = 0, Red, DarkBlue }

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
	public void CanConvert_OnlyEnums()
	{
		var factory = new TolerantEnumConverterFactory();
		factory.CanConvert(typeof(Colour)).Should().BeTrue();
		factory.CanConvert(typeof(string)).Should().BeFalse();
	}
}
