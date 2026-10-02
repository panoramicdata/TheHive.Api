using System.Text.Json;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Query;
using TheHive.Api.Querying;

namespace TheHive.Api.Test.Integration;

public class QueryTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task QueryBuilder_FindsTheTestCaseWithEveryStepKind()
	{
		var client = Client;
		var title = NewName();
		string? caseId = null;
		try
		{
			var created = await client.Cases.CreateAsync(
				new CaseCreateRequest
				{
					Title = title,
					Description = "Created by the TheHive.Api integration tests; safe to delete.",
					Severity = Severity.Low,
					Tags = ["integration-test"]
				},
				CancellationToken);
			caseId = created.Id;
			var byTitle = QueryBuilder.ListCases().Filter("title", title);

			// _eq, with a query name
			var found = await client.Query.RunAsync<Case>(byTitle, "integration-test", CancellationToken);
			found.Should().ContainSingle().Which.Title.Should().Be(title);
			found[0].Id.Should().Be(caseId);

			// count
			(await client.Query.RunCountAsync(QueryBuilder.ListCases().Filter("title", title).Count(), cancellationToken: CancellationToken))
				.Should().Be(1);

			// _in, _and, _or, _not
			(await CountAsync(QueryBuilder.ListCases().FilterIn("title", title, "[TheHive.Api integration] no such case"))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().And(f => f.Eq("title", title).Eq("severity", Severity.Low)))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().And(f => f.Eq("title", title).Eq("severity", Severity.High)))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).Or(f => f.Eq("severity", Severity.High).Has("tags")))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).Not(f => f.Eq("severity", Severity.High)))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).Not(f => f.Eq("severity", Severity.Low)))).Should().Be(0);

			// _like, _startsWith, _between, _gt, _lt (dates as DateTimeOffset)
			var guid = title[(title.LastIndexOf(' ') + 1)..];
			(await CountAsync(QueryBuilder.ListCases().FilterLike("title", guid))).Should().Be(1);

			// _like: a case-sensitive substring, or a whole word in any case; no wildcard needed, outer * changes nothing, inner * is not a wildcard.
			var token = guid.Split('-')[0];
			var part = token[1..6];
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", part))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", "*" + part + "*"))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", token[..3] + "*" + token[5..]))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", "ntegrat"))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", "NTEGRAT"))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLike("title", "INTEGRATION"))).Should().Be(1);

			// _between: _from inclusive, _to exclusive (numbers and dates)
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterBetween("severity", Severity.Low, Severity.Medium))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterBetween("severity", 0, Severity.Low))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterBetween("number", created.Number, created.Number + 1))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterBetween("number", created.Number - 1, created.Number))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title)
				.FilterBetween("_createdAt", created.CreatedAt.AddMilliseconds(-1), created.CreatedAt))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title)
				.FilterBetween("_createdAt", created.CreatedAt, created.CreatedAt.AddMilliseconds(1)))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).Filter(f => f.StartsWith("title", "[TheHive.Api integration]")))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title)
				.FilterBetween("_createdAt", created.CreatedAt.AddMinutes(-10), created.CreatedAt.AddMinutes(10)))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterGt("_createdAt", created.CreatedAt.AddMinutes(-10)))).Should().Be(1);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title).FilterLt("_createdAt", created.CreatedAt.AddMinutes(-10)))).Should().Be(0);
			(await CountAsync(QueryBuilder.ListCases().Filter("title", title)
				.FilterGt("_createdAt", new { amount = 1, unit = "days", look = "behind" }))).Should().Be(1);

			// sort, page with total, select
			var page = await client.Query.RunPageAsync<Case>(
				QueryBuilder.ListCases().Filter("title", title).Sort("_createdAt", SortDirection.Descending).Sort("title").Page(0, 5, "total").Select("title", "_id"),
				"integration-test",
				CancellationToken);
			page.Total.Should().Be(1);
			page.Items.Should().ContainSingle().Which.Title.Should().Be(title);

			// get, related, raw result
			var single = await client.Query.RunAsync<Case>(QueryBuilder.GetCase(caseId), cancellationToken: CancellationToken);
			single.Should().ContainSingle().Which.Id.Should().Be(caseId);
			var tasks = await client.Query.RunAsync(QueryBuilder.GetCase(caseId).Related("tasks").Build(), cancellationToken: CancellationToken);
			tasks.ValueKind.Should().Be(JsonValueKind.Array);
			tasks.GetArrayLength().Should().Be(0);

			// export (read-only) of the one test case
			using var export = await client.Query.ExportAsync(byTitle, new ExportOptions { Format = ExportFormat.Json, Model = ExportModel.Case }, CancellationToken);
			using var exported = JsonDocument.Parse(await export.ReadAsStringAsync(CancellationToken));
			exported.RootElement.GetArrayLength().Should().Be(1);
			exported.RootElement[0].GetProperty("title").GetString().Should().Be(title);
		}
		finally
		{
			if (caseId is not null)
			{
				await TryCleanupAsync(() => client.Cases.DeleteAsync(caseId, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task GetExportFields_ListsCaseFields()
	{
		var fields = await Client.Query.GetExportFieldsAsync(CancellationToken);

		fields.FieldsByModel.Should().ContainKey("Case");
		fields.FieldsByModel["Case"].Should().Contain(f => f.FieldPath == "title");
	}

	private Task<long> CountAsync(QueryBuilder builder) => Client.Query.RunCountAsync(builder.Count(), cancellationToken: CancellationToken);
}
