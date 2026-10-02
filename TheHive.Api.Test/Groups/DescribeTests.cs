using System.Net;
using TheHive.Api.Data.Describe;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class DescribeTests
{
	// The spec's modelDescriptions example's "case" entry, plus one property of each remaining kind.
	private const string CaseDescriptionJson = """
		{
			"label":"case","path":"","initialQuery":"listCase",
			"attributes":[
				{"name":"customFields.threat-type","cardinality":"list","aggregable":true,"indexType":"fulltext","type":"string"},
				{"name":"severity","cardinality":"single","values":[1,2,3,4],"labels":["low","medium","high","critical"],"aggregable":true,"indexType":"standard","type":"enumeration"},
				{"name":"assignee","cardinality":"option","aggregable":true,"indexType":"standard","type":"user"},
				{"name":"tags","cardinality":"set","aggregable":false,"indexType":"none","type":"string"},
				{"name":"summary","cardinality":"option","aggregable":false,"indexType":"fulltextOnly","type":"string"},
				{"name":"status","cardinality":"single","values":["New","InProgress"],"aggregable":true,"indexType":"standard","type":"enumeration"},
				{"name":"future","cardinality":"many","aggregable":false,"indexType":"vector","type":"geo"}
			]
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json)
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertCaseDescription(EntityDescription description)
	{
		description.Label.Should().Be("case");
		description.Path.Should().BeEmpty();
		description.InitialQuery.Should().Be("listCase");
		description.Attributes.Should().HaveCount(7);

		var threat = description.Attributes[0];
		threat.Name.Should().Be("customFields.threat-type");
		threat.Cardinality.Should().Be(PropertyCardinality.List);
		threat.Aggregable.Should().BeTrue();
		threat.IndexType.Should().Be(PropertyIndexType.Fulltext);
		threat.Type.Should().Be("string");
		threat.Values.Should().BeEmpty();
		threat.Labels.Should().BeEmpty();

		var severity = description.Attributes[1];
		severity.Name.Should().Be("severity");
		severity.Cardinality.Should().Be(PropertyCardinality.Single);
		severity.Values.Select(v => v.GetInt32()).Should().Equal(1, 2, 3, 4);
		severity.Labels.Should().Equal("low", "medium", "high", "critical");
		severity.IndexType.Should().Be(PropertyIndexType.Standard);
		severity.Type.Should().Be("enumeration");

		description.Attributes[2].Cardinality.Should().Be(PropertyCardinality.Option);
		description.Attributes[2].Type.Should().Be("user");
		description.Attributes[3].Cardinality.Should().Be(PropertyCardinality.Set);
		description.Attributes[3].Aggregable.Should().BeFalse();
		description.Attributes[3].IndexType.Should().Be(PropertyIndexType.None);
		description.Attributes[4].IndexType.Should().Be(PropertyIndexType.FulltextOnly);
		description.Attributes[5].Values.Select(v => v.GetString()).Should().Equal("New", "InProgress");

		var future = description.Attributes[6];
		future.Cardinality.Should().Be(PropertyCardinality.Unknown);
		future.IndexType.Should().Be(PropertyIndexType.Unknown);
		future.Type.Should().Be("geo");
	}

	[Fact]
	public async Task GetAllAsync_MapsEveryModel()
	{
		var stub = Stub(HttpStatusCode.OK, $$$"""{"case":{{{CaseDescriptionJson}}},"alert":{"label":"alert","path":"","initialQuery":"listAlert","attributes":[]}}""");
		using var client = TestClient.Create(stub);

		var result = await client.Describe.GetAllAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/describe/_all");
		result.Keys.Should().Equal("case", "alert");
		AssertCaseDescription(result["case"]);
		result["alert"].InitialQuery.Should().Be("listAlert");
		result["alert"].Attributes.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_MapsTheModel()
	{
		var stub = Stub(HttpStatusCode.OK, CaseDescriptionJson);
		using var client = TestClient.Create(stub);

		var result = await client.Describe.GetAsync("case", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/describe/case");
		AssertCaseDescription(result);
	}

	[Fact]
	public async Task GetAllAsync_Forbidden_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.Forbidden, """{"type":"AuthorizationError","message":"Not allowed"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Describe.GetAllAsync(TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.Forbidden && e.ErrorType == "AuthorizationError");
	}

	[Fact]
	public async Task GetAsync_UnknownModel_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Model hologram not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Describe.GetAsync("hologram", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
