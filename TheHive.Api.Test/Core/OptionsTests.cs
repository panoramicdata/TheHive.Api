namespace TheHive.Api.Test.Core;

public class OptionsTests
{
	private static TheHiveClientOptions Valid() => new() { BaseUrl = "https://hive.test", ApiKey = "fake-key" };

	[Fact]
	public void Validate_EmptyBaseUrl_Throws()
	{
		var options = Valid();
		options.BaseUrl = string.Empty;
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Validate_RelativeBaseUrl_Throws()
	{
		var options = Valid();
		options.BaseUrl = "/relative/path";
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Validate_EmptyApiKey_Throws()
	{
		var options = Valid();
		options.ApiKey = string.Empty;
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentException>();
	}

	[Fact]
	public void Validate_NegativeMaxRetries_Throws()
	{
		var options = Valid();
		options.MaxRetries = -1;
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentOutOfRangeException>();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	public void Validate_NonPositiveTimeout_Throws(int seconds)
	{
		var options = Valid();
		options.Timeout = TimeSpan.FromSeconds(seconds);
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentOutOfRangeException>();
	}


	[Fact]
	public void Validate_NegativeRetryBaseDelay_Throws()
	{
		var options = Valid();
		options.RetryBaseDelay = TimeSpan.FromSeconds(-1);
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void Validate_ZeroRetryBaseDelay_DoesNotThrow()
	{
		var options = Valid();
		options.RetryBaseDelay = TimeSpan.Zero;
		options.Invoking(o => o.Validate()).Should().NotThrow();
	}

	[Theory]
	[InlineData(0)]
	[InlineData(-5)]
	public void Validate_NonPositiveMaxRetryDelay_Throws(int seconds)
	{
		var options = Valid();
		options.MaxRetryDelay = TimeSpan.FromSeconds(seconds);
		options.Invoking(o => o.Validate()).Should().Throw<ArgumentOutOfRangeException>();
	}

	[Fact]
	public void MaxRetryDelay_DefaultsToThirtySeconds()
		=> Valid().MaxRetryDelay.Should().Be(TimeSpan.FromSeconds(30));
	[Fact]
	public void Validate_ValidOptions_DoesNotThrow()
		=> Valid().Invoking(o => o.Validate()).Should().NotThrow();

	[Fact]
	public void ToString_DoesNotContainApiKey()
	{
		var text = Valid().ToString();
		text.Should().NotContain("fake-key");
		text.Should().Contain("https://hive.test");
	}
}
