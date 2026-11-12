namespace TheHive.Api.Test.Support;

internal static class StubAssertions
{
	public static void ShouldHaveFirstCall(this StubHandler stub, HttpMethod method, string path, string? body)
	{
		stub.Calls[0].Method.Should().Be(method);
		stub.Calls[0].Uri.AbsolutePath.Should().Be(path);
		stub.Calls[0].Body.Should().Be(body);
	}
}
