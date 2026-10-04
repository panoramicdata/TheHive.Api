using System.Net;
using Refit;
using TheHive.Api.Querying;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Core;

/// <summary>
/// The methods that unpack an options object into a raw Refit transport (multipart form fields and request headers) and the query helpers
/// reject a <see langword="null"/> options object before sending anything.
/// </summary>
public class OptionsObjectTests
{
	private static readonly MultipartItem[] Files = [new ByteArrayPart([1], "a.bin", "application/octet-stream")];

	public static TheoryData<string> Wrappers =>
	[
		"Alerts.AddAttachmentsAsync",
		"Cases.AddAttachmentsAsync",
		"CaseReportTemplates.AddAttachmentsAsync",
		"Organisations.UploadAttachmentsAsync",
		"Users.UploadTemporaryAttachmentsAsync",
		"Branding.GetAssetAsync",
		"CaseReportTemplates.GetAttachmentAsync",
		"Organisations.GetAvatarAsync",
		"Organisations.GetAttachmentAsync",
		"TaskLogs.GetObservableAttachmentAsync",
		"Users.GetAvatarAsync",
		"Query.RunAsync<T>",
		"Query.RunPageAsync<T>",
		"Query.RunCountAsync"
	];

	[Theory]
	[MemberData(nameof(Wrappers))]
	public async Task NullOptions_ThrowsWithoutSending(string method)
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);
		var token = TestContext.Current.CancellationToken;

		Func<Task> act = method switch
		{
			"Alerts.AddAttachmentsAsync" => () => client.Alerts.AddAttachmentsAsync("~1", Files, null!, token),
			"Cases.AddAttachmentsAsync" => () => client.Cases.AddAttachmentsAsync("~1", Files, null!, token),
			"CaseReportTemplates.AddAttachmentsAsync" => () => client.CaseReportTemplates.AddAttachmentsAsync("~1", Files, null!, token),
			"Organisations.UploadAttachmentsAsync" => () => client.Organisations.UploadAttachmentsAsync(Files, null!, token),
			"Users.UploadTemporaryAttachmentsAsync" => () => client.Users.UploadTemporaryAttachmentsAsync(Files, null!, token),
			"Branding.GetAssetAsync" => () => client.Branding.GetAssetAsync("favicon", null!, token),
			"CaseReportTemplates.GetAttachmentAsync" => () => client.CaseReportTemplates.GetAttachmentAsync("~1", "~2", null!, token),
			"Organisations.GetAvatarAsync" => () => client.Organisations.GetAvatarAsync("~1", "hash", null!, token),
			"Organisations.GetAttachmentAsync" => () => client.Organisations.GetAttachmentAsync("~1", null!, token),
			"TaskLogs.GetObservableAttachmentAsync" => () => client.TaskLogs.GetObservableAttachmentAsync("~1", "~2", null!, token),
			"Users.GetAvatarAsync" => () => client.Users.GetAvatarAsync("~1", "file", null!, token),
			"Query.RunAsync<T>" => () => client.Query.RunAsync<object>(QueryBuilder.ListCases(), null!, token),
			"Query.RunPageAsync<T>" => () => client.Query.RunPageAsync<object>(QueryBuilder.ListCases(), null!, token),
			"Query.RunCountAsync" => () => client.Query.RunCountAsync(QueryBuilder.ListCases().Count(), null!, token),
			_ => throw new ArgumentOutOfRangeException(nameof(method), method, "Unknown method.")
		};

		(await act.Should().ThrowAsync<ArgumentNullException>()).Which.ParamName.Should().Be("options");
		stub.Calls.Should().BeEmpty();
	}

	// A null [Query] options object on a Refit-generated method is sent as an empty one (no query string). These pin that
	// Refit behaviour, one per kind of request (GET, DELETE, POST with a body), so a Refit upgrade that changes it is noticed.

	[Fact]
	public async Task NullQueryObject_OnAGet_SendsNoQueryString()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.Status.GetAsync(null!, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/status");
	}

	[Fact]
	public async Task NullQueryObject_OnADelete_SendsNoQueryString()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Users.DeleteAsync("~1", null!, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/user/~1/force");
	}

	[Fact]
	public async Task NullQueryObject_OnAPostWithABody_SendsNoQueryString()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK);
		using var client = TestClient.Create(stub);

		await client.Functions.TestAsync(new Data.Functions.FunctionTestRequest { Definition = "x" }, null!, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.PathAndQuery.Should().Be("/api/v1/function/_test");
	}
}