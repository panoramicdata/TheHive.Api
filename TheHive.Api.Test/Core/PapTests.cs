using TheHive.Api.Data.Common;

namespace TheHive.Api.Test.Core;

public class PapTests
{
	[Fact]
	public void Levels_HaveTheWireValues()
	{
		Pap.Clear.Should().Be(0);
		Pap.Green.Should().Be(1);
		Pap.Amber.Should().Be(2);
		Pap.Red.Should().Be(3);
	}
}
