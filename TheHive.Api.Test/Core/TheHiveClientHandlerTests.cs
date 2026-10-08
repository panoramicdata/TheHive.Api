namespace TheHive.Api.Test.Core;

public class TheHiveClientHandlerTests
{
	[Fact]
	public void CreateDefaultHandler_ByDefault_ValidatesCertificates()
	{
		using var handler = TheHiveClient.CreateDefaultHandler(new TheHiveClientOptions());

		handler.ServerCertificateCustomValidationCallback.Should().BeNull();
	}

	[Fact]
	public void CreateDefaultHandler_IgnoreCertificateErrors_AcceptsAnyCertificate()
	{
		using var handler = TheHiveClient.CreateDefaultHandler(new TheHiveClientOptions { IgnoreCertificateErrors = true });

		handler.ServerCertificateCustomValidationCallback
			.Should().BeSameAs(HttpClientHandler.DangerousAcceptAnyServerCertificateValidator);
	}

	[Fact]
	public void CreateDefaultHandler_NullOptions_Throws()
		=> FluentActions.Invoking(() => TheHiveClient.CreateDefaultHandler(null!))
			.Should().Throw<ArgumentNullException>();

	[Fact]
	public void ToString_ShowsIgnoreCertificateErrorsButNotTheKey()
		=> new TheHiveClientOptions { BaseUrl = "https://h", ApiKey = "secret-key", IgnoreCertificateErrors = true }
			.ToString().Should().Contain("IgnoreCertificateErrors = True").And.NotContain("secret-key");
}
