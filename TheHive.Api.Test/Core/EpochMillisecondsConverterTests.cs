using System.Text.Json;
using TheHive.Api.Converters;

namespace TheHive.Api.Test.Core;

public class EpochMillisecondsConverterTests
{
	private static readonly JsonSerializerOptions Options = new() { Converters = { new EpochMillisecondsConverter() } };

	[Fact]
	public void Read_Number_ReturnsUtcInstant() =>
		JsonSerializer.Deserialize<DateTimeOffset>("1700000000123", Options)
			.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000000123));

	[Fact]
	public void Read_NullableNull_ReturnsNull() =>
		JsonSerializer.Deserialize<DateTimeOffset?>("null", Options).Should().BeNull();

	[Fact]
	public void Read_String_Throws()
	{
		var act = () => JsonSerializer.Deserialize<DateTimeOffset>("\"x\"", Options);
		act.Should().Throw<JsonException>();
	}

	[Fact]
	public void Write_RoundTripsMilliseconds() =>
		JsonSerializer.Serialize(DateTimeOffset.FromUnixTimeMilliseconds(1700000000123), Options)
			.Should().Be("1700000000123");
}
