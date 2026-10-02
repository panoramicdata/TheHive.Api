namespace TheHive.Api.Test.Integration;

public class DescribeTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task Describe_ReturnsTheCaseModel()
	{
		var all = await Client.Describe.GetAllAsync(CancellationToken);
		var model = await Client.Describe.GetAsync("case", CancellationToken);

		all.Should().ContainKey("case");
		all["case"].Attributes.Should().NotBeEmpty();
		model.Label.Should().Be("case");
		model.InitialQuery.Should().Be("listCase");
		model.Attributes.Should().Contain(a => a.Name == "title");
		model.Attributes.Should().Contain(a => a.Name == "severity" && a.Values.Count > 0);
	}
}
