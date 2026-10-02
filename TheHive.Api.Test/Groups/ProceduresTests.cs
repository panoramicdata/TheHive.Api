using System.Net;
using TheHive.Api.Data.Procedures;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class ProceduresTests
{
	private const string FullProcedureJson = """
		{
			"_id":"~234567890","_createdAt":1748739600000,"_createdBy":"analyst@example.com",
			"_updatedAt":1748826000000,"_updatedBy":"lead@example.com",
			"description":"PowerShell script used to encrypt files.","occurDate":1748739600000,
			"patternId":"T1486","patternName":"Data Encrypted for Impact","tactic":"impact","tacticLabel":"Impact",
			"extraData":{"note":"x"}
		}
		""";

	private const string MinimalProcedureJson = """
		{"_id":"~1","_createdAt":1748739600000,"_createdBy":"a@example.com","occurDate":1748739600000}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static ProcedureInput Input() => new()
	{
		PatternId = "T1486",
		OccurDate = DateTimeOffset.FromUnixTimeMilliseconds(1748739600000),
		Tactic = "impact",
		Description = "d"
	};

	private const string InputJson = """{"patternId":"T1486","occurDate":1748739600000,"tactic":"impact","description":"d"}""";

	private static void AssertFull(Procedure item)
	{
		item.Id.Should().Be("~234567890");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.CreatedBy.Should().Be("analyst@example.com");
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748826000000));
		item.UpdatedBy.Should().Be("lead@example.com");
		item.Description.Should().Be("PowerShell script used to encrypt files.");
		item.OccurDate.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748739600000));
		item.PatternId.Should().Be("T1486");
		item.PatternName.Should().Be("Data Encrypted for Impact");
		item.Tactic.Should().Be("impact");
		item.TacticLabel.Should().Be("Impact");
		item.ExtraData.Should().ContainKey("note");
	}

	[Fact]
	public async Task CreateForAlertAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullProcedureJson);
		using var client = TestClient.Create(stub);

		var result = await client.Procedures.CreateForAlertAsync("~354", Input(), TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/procedure");
		stub.Calls[0].Body.Should().Be(InputJson);
		AssertFull(result);
	}

	[Fact]
	public async Task CreateForCaseAsync_PostsRequiredFieldsOnly_AndMapsAbsentOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalProcedureJson);
		using var client = TestClient.Create(stub);

		var result = await client.Procedures.CreateForCaseAsync(
			"12",
			new ProcedureInput { PatternId = "T1486", OccurDate = DateTimeOffset.FromUnixTimeMilliseconds(1748739600000) },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/12/procedure");
		stub.Calls[0].Body.Should().Be("""{"patternId":"T1486","occurDate":1748739600000}""");
		result.Id.Should().Be("~1");
		result.Description.Should().BeNull();
		result.PatternId.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task CreateManyForAlertAsync_PostsProceduresArray_AndMapsList()
	{
		var stub = Stub(HttpStatusCode.Created, $"[{FullProcedureJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Procedures.CreateManyForAlertAsync(
			"~354",
			new ProcedureBulkCreateRequest { Procedures = [Input()] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/alert/~354/procedures");
		stub.Calls[0].Body.Should().Be($$"""{"procedures":[{{InputJson}}]}""");
		AssertFull(result.Should().ContainSingle().Which);
	}

	[Fact]
	public async Task CreateManyForCaseAsync_PostsProceduresArray_AndMapsList()
	{
		var stub = Stub(HttpStatusCode.Created, $"[{FullProcedureJson}]");
		using var client = TestClient.Create(stub);

		var result = await client.Procedures.CreateManyForCaseAsync(
			"~354",
			new ProcedureBulkCreateRequest { Procedures = [Input()] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~354/procedures");
		stub.Calls[0].Body.Should().Be($$"""{"procedures":[{{InputJson}}]}""");
		AssertFull(result.Should().ContainSingle().Which);
	}

	[Fact]
	public void ProcedureBulkCreateRequest_DefaultsToNoProcedures() =>
		new ProcedureBulkCreateRequest().Procedures.Should().BeEmpty();

	[Fact]
	public async Task UpdateAsync_PatchesEveryField()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Procedures.UpdateAsync(
			"~234567890",
			new ProcedureUpdateRequest
			{
				Description = "new",
				OccurDate = DateTimeOffset.FromUnixTimeMilliseconds(1748739600000),
				PatternId = "T1059.001",
				Tactic = "execution"
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/procedure/~234567890");
		stub.Calls[0].Body.Should().Be("""{"description":"new","occurDate":1748739600000,"patternId":"T1059.001","tactic":"execution"}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Procedures.UpdateAsync("~1", new ProcedureUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Procedures.DeleteAsync("~234567890", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/procedure/~234567890");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task BulkDeleteAsync_PostsIds()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.Procedures.BulkDeleteAsync(new ProcedureBulkDeleteRequest { Ids = ["~128458762", "~216513541"] }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/procedure/delete/_bulk");
		stub.Calls[0].Body.Should().Be("""{"ids":["~128458762","~216513541"]}""");
	}

	[Fact]
	public async Task CreateForCaseAsync_UnknownPattern_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.BadRequest, """{"type":"BadRequest","message":"Unknown pattern"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Procedures.CreateForCaseAsync("~354", Input(), TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.BadRequest && e.ErrorType == "BadRequest");
	}
}
