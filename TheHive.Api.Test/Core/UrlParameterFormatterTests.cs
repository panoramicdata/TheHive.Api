using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

public class UrlParameterFormatterTests
{
	private static string? Format(object? value, Type type)
		=> new TheHiveUrlParameterFormatter().Format(value, typeof(UrlParameterFormatterTests), type);

	[Fact]
	public void Format_True_IsLowercase() => Format(true, typeof(bool)).Should().Be("true");

	[Fact]
	public void Format_False_IsLowercase() => Format(false, typeof(bool?)).Should().Be("false");

	[Fact]
	public void Format_Null_StaysNull() => Format(null, typeof(bool?)).Should().BeNull();

	[Fact]
	public void Format_NonBoolValues_DeferToRefit()
	{
		Format(42, typeof(int)).Should().Be("42");
		Format("a b", typeof(string)).Should().Be("a b");
	}

	[Fact]
	public void Client_UsesTheFormatter()
	{
		using var client = TestClient.Create(new StubHandler());

		client.Settings.UrlParameterFormatter.Should().BeOfType<TheHiveUrlParameterFormatter>();
	}
}
