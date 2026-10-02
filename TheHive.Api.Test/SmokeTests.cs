namespace TheHive.Api.Test;

public class SmokeTests
{
	[Fact]
	public void Assembly_Loads() => typeof(TheHive.Api.TheHiveMarker).Assembly.Should().NotBeNull();
}
