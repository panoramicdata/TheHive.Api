using System.Text.Json;
using TheHive.Api.Querying;

namespace TheHive.Api.Test.Querying;

public class QueryBuilderTests
{
	private static string Json(QueryBuilder builder) => JsonSerializer.Serialize(builder.Build(), TheHiveJson.Options);

	[Fact]
	public void List_FilterSortPage_ProducesTheSpecShape()
	{
		var builder = QueryBuilder.ListCases()
			.Filter("severity", 3)
			.Sort("_createdAt", SortDirection.Descending)
			.Page(0, 10, "taskStats", "shareCount", "total")
			.Exclude("description");

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"severity","_value":3}},{"_name":"sort","_fields":[{"_createdAt":"desc"}]},{"_name":"page","from":0,"to":10,"extraData":["taskStats","shareCount","total"]}],"excludeFields":["description"]}""");
	}

	[Theory]
	[InlineData("ListCases", "listCase")]
	[InlineData("ListAlerts", "listAlert")]
	[InlineData("ListTasks", "listTask")]
	[InlineData("ListObservables", "listObservable")]
	[InlineData("ListTaskLogs", "listLog")]
	[InlineData("ListComments", "listComment")]
	[InlineData("ListUsers", "listUser")]
	[InlineData("ListOrganisations", "listOrganisation")]
	[InlineData("ListCaseTemplates", "listCaseTemplate")]
	[InlineData("ListCustomFields", "listCustomField")]
	[InlineData("ListProcedures", "listProcedure")]
	[InlineData("ListPages", "listPage")]
	[InlineData("ListDashboards", "listDashboard")]
	[InlineData("ListProfiles", "listProfile")]
	[InlineData("ListTags", "listTag")]
	public void TypedListStarters_UseTheDocumentedOperationName(string method, string operation)
	{
		var builder = (QueryBuilder)typeof(QueryBuilder).GetMethod(method)!.Invoke(null, null)!;

		Json(builder).Should().Be($$"""{"query":[{"_name":"{{operation}}"}]}""");
	}

	[Theory]
	[InlineData("GetCase", "getCase")]
	[InlineData("GetAlert", "getAlert")]
	[InlineData("GetTask", "getTask")]
	[InlineData("GetObservable", "getObservable")]
	[InlineData("GetTaskLog", "getLog")]
	[InlineData("GetUser", "getUser")]
	[InlineData("GetOrganisation", "getOrganisation")]
	public void TypedGetStarters_SendIdOrName(string method, string operation)
	{
		var builder = (QueryBuilder)typeof(QueryBuilder).GetMethod(method)!.Invoke(null, ["~1234"])!;

		Json(builder).Should().Be($$"""{"query":[{"_name":"{{operation}}","idOrName":"~1234"}]}""");
	}

	[Fact]
	public void List_And_Get_AcceptAnyPrimaryOperation()
	{
		Json(QueryBuilder.List("myTasks")).Should().Be("""{"query":[{"_name":"myTasks"}]}""");
		Json(QueryBuilder.Get("getCaseTemplate", "Default")).Should().Be("""{"query":[{"_name":"getCaseTemplate","idOrName":"Default"}]}""");
	}

	[Fact]
	public void Related_ChainsRelatedObjectOperations()
	{
		var builder = QueryBuilder.GetCase("~1234").Related("tasks").Related("logs");

		Json(builder).Should().Be("""{"query":[{"_name":"getCase","idOrName":"~1234"},{"_name":"tasks"},{"_name":"logs"}]}""");
	}

	[Fact]
	public void FilterShortcuts_ProduceOneFilterStepEach()
	{
		var builder = QueryBuilder.ListCases()
			.Filter("status", "New")
			.FilterIn("severity", 2, 3)
			.FilterLike("title", "ransom")
			.FilterGt("number", 10)
			.FilterLt("number", 20)
			.FilterBetween("_createdAt", 1000, 2000);

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},""" +
			"""{"_name":"filter","_eq":{"_field":"status","_value":"New"}},""" +
			"""{"_name":"filter","_in":{"_field":"severity","_values":[2,3]}},""" +
			"""{"_name":"filter","_like":{"_field":"title","_value":"ransom"}},""" +
			"""{"_name":"filter","_gt":{"_field":"number","_value":10}},""" +
			"""{"_name":"filter","_lt":{"_field":"number","_value":20}},""" +
			"""{"_name":"filter","_between":{"_field":"_createdAt","_from":1000,"_to":2000}}]}""");
	}

	[Fact]
	public void Filter_NullValue_And_DateValue_UseTheClientConverters()
	{
		var builder = QueryBuilder.ListCases()
			.Filter("assignee", null)
			.FilterGt("_createdAt", DateTimeOffset.FromUnixTimeMilliseconds(1734425224596));

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"assignee","_value":null}},{"_name":"filter","_gt":{"_field":"_createdAt","_value":1734425224596}}]}""");
	}

	[Fact]
	public void Filter_LevelConstantsAndEnums_WriteTheServerForm()
	{
		// Severity, TLP and PAP are int constants, so the server's integer is written; tolerant enums write their wire name.
		var builder = QueryBuilder.ListCases()
			.Filter("severity", TheHive.Api.Data.Common.Severity.High)
			.Filter("stage", TheHive.Api.Data.Cases.CaseStage.Closed);

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"severity","_value":3}},{"_name":"filter","_eq":{"_field":"stage","_value":"Closed"}}]}""");
	}

	[Fact]
	public void FilterWithRelativeDate_WritesTheObjectAsTheValue()
	{
		var builder = QueryBuilder.ListCases()
			.FilterGt("_createdAt", new { amount = 0, unit = "days", look = "behind", modifier = "startOfDay" });

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"filter","_gt":{"_field":"_createdAt","_value":{"amount":0,"unit":"days","look":"behind","modifier":"startOfDay"}}}]}""");
	}

	[Fact]
	public void AndOrNot_GroupConditionsInOneFilterStep()
	{
		var builder = QueryBuilder.ListCases()
			.And(f => f.Eq("status", "New").Gte("severity", 2))
			.Or(f => f.Eq("tlp", 1).Eq("tlp", 2))
			.Not(f => f.Eq("flag", true));

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},""" +
			"""{"_name":"filter","_and":[{"_eq":{"_field":"status","_value":"New"}},{"_gte":{"_field":"severity","_value":2}}]},""" +
			"""{"_name":"filter","_or":[{"_eq":{"_field":"tlp","_value":1}},{"_eq":{"_field":"tlp","_value":2}}]},""" +
			"""{"_name":"filter","_not":{"_eq":{"_field":"flag","_value":true}}}]}""");
	}

	[Fact]
	public void Filter_WithBuilder_SingleConditionIsInlined_SeveralAreAnded()
	{
		var single = QueryBuilder.ListAlerts().Filter(f => f.Has("assignee"));
		var several = QueryBuilder.ListAlerts().Filter(f => f.Has("assignee").Ne("stage", "Closed"));

		Json(single).Should().Be("""{"query":[{"_name":"listAlert"},{"_name":"filter","_has":"assignee"}]}""");
		Json(several).Should().Be(
			"""{"query":[{"_name":"listAlert"},{"_name":"filter","_and":[{"_has":"assignee"},{"_ne":{"_field":"stage","_value":"Closed"}}]}]}""");
	}

	[Fact]
	public void Sort_ConsecutiveCallsShareOneSortStep_AscendingByDefault()
	{
		var builder = QueryBuilder.ListCases().Sort("title").Sort("_createdAt", SortDirection.Descending);

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"sort","_fields":[{"title":"asc"},{"_createdAt":"desc"}]}]}""");
	}

	[Fact]
	public void Sort_AfterAnotherStep_StartsANewSortStep()
	{
		var builder = QueryBuilder.ListCases().Sort("title").Related("tasks").Sort("dueDate");

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"sort","_fields":[{"title":"asc"}]},{"_name":"tasks"},{"_name":"sort","_fields":[{"dueDate":"asc"}]}]}""");
	}

	[Fact]
	public void Page_WithoutExtraData_OmitsIt()
	{
		Json(QueryBuilder.ListCases().Page(0, 15)).Should().Be("""{"query":[{"_name":"listCase"},{"_name":"page","from":0,"to":15}]}""");
	}

	[Fact]
	public void Count_EndsTheQuery()
	{
		var builder = QueryBuilder.ListCases().Filter("status", "New").Count();

		Json(builder).Should().Be("""{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"status","_value":"New"}},{"_name":"count"}]}""");
		builder.EndsWithCount.Should().BeTrue();
		QueryBuilder.ListCases().EndsWithCount.Should().BeFalse();
	}

	[Fact]
	public void Select_And_Exclude_SetIncludeAndExcludeFields()
	{
		var builder = QueryBuilder.ListCases().Select("title", "_id").Select("number").Exclude("description");

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"}],"includeFields":["title","_id","number"],"excludeFields":["description"]}""");
	}

	[Fact]
	public void Build_ReturnsIndependentCopies()
	{
		var builder = QueryBuilder.ListCases().Sort("title");
		var first = builder.Build();
		first.Query[1]["_fields"]!.AsArray().Clear();

		builder.Sort("number");
		var second = builder.Build();

		JsonSerializer.Serialize(second, TheHiveJson.Options).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"sort","_fields":[{"title":"asc"},{"number":"asc"}]}]}""");
	}

	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData(" ")]
	public void List_BlankName_Throws(string? name)
	{
		var act = () => QueryBuilder.List(name!);

		act.Should().Throw<ArgumentException>();
	}

	[Theory]
	[InlineData("filter")]
	[InlineData("sort")]
	[InlineData("page")]
	[InlineData("count")]
	[InlineData("output")]
	public void List_StartingWithANonPrimaryStep_Throws(string name)
	{
		var act = () => QueryBuilder.List(name);

		act.Should().Throw<ArgumentException>().WithMessage("*primary operation*");
	}

	[Fact]
	public void Get_BlankIdOrName_Throws()
	{
		var act = () => QueryBuilder.GetCase(" ");

		act.Should().Throw<ArgumentException>().WithParameterName("idOrName");
	}

	[Fact]
	public void Get_NonPrimaryName_Throws()
	{
		var act = () => QueryBuilder.Get("sort", "~1");

		act.Should().Throw<ArgumentException>().WithMessage("*primary operation*");
	}

	[Fact]
	public void Related_BlankName_Throws()
	{
		var act = () => QueryBuilder.ListCases().Related("");

		act.Should().Throw<ArgumentException>().WithParameterName("operationName");
	}

	[Fact]
	public void Filter_BlankField_Throws()
	{
		var act = () => QueryBuilder.ListCases().Filter(" ", "x");

		act.Should().Throw<ArgumentException>().WithParameterName("field");
	}

	[Fact]
	public void Filter_EmptyBuilder_Throws()
	{
		var act = () => QueryBuilder.ListCases().Filter(_ => { });

		act.Should().Throw<ArgumentException>().WithMessage("*at least one condition*");
	}

	[Fact]
	public void Filter_NullAction_Throws()
	{
		var act = () => QueryBuilder.ListCases().Filter(null!);

		act.Should().Throw<ArgumentNullException>();
	}

	[Fact]
	public void Sort_BlankField_Throws()
	{
		var act = () => QueryBuilder.ListCases().Sort("");

		act.Should().Throw<ArgumentException>().WithParameterName("field");
	}

	[Fact]
	public void Sort_UndefinedDirection_Throws()
	{
		var act = () => QueryBuilder.ListCases().Sort("title", (SortDirection)7);

		act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("direction");
	}

	[Theory]
	[InlineData(-1, 10, "from")]
	[InlineData(5, 2, "to")]
	[InlineData(5, 5, "to")]
	public void Page_InvalidRange_Throws(int from, int to, string parameter)
	{
		var act = () => QueryBuilder.ListCases().Page(from, to);

		act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName(parameter);
	}

	[Fact]
	public void Page_BlankExtraData_Throws()
	{
		var act = () => QueryBuilder.ListCases().Page(0, 10, "total", " ");

		act.Should().Throw<ArgumentException>().WithParameterName("extraData");
	}

	[Fact]
	public void Select_NoFields_Throws()
	{
		var act = () => QueryBuilder.ListCases().Select();

		act.Should().Throw<ArgumentException>().WithParameterName("fields");
	}

	[Fact]
	public void Exclude_BlankField_Throws()
	{
		var act = () => QueryBuilder.ListCases().Exclude("title", "");

		act.Should().Throw<ArgumentException>().WithParameterName("fields");
	}

	[Fact]
	public void StepsAfterPage_Throw()
	{
		var builder = QueryBuilder.ListCases().Page(0, 10);

		var act = () => builder.Filter("status", "New");

		act.Should().Throw<InvalidOperationException>().WithMessage("*page*last step*");
	}

	[Fact]
	public void StepsAfterCount_Throw()
	{
		var builder = QueryBuilder.ListCases().Count();

		var sort = () => builder.Sort("title");
		var related = () => builder.Related("tasks");
		var page = () => builder.Page(0, 1);
		var count = () => builder.Count();

		sort.Should().Throw<InvalidOperationException>().WithMessage("*count*last step*");
		related.Should().Throw<InvalidOperationException>();
		page.Should().Throw<InvalidOperationException>();
		count.Should().Throw<InvalidOperationException>();
	}

	[Fact]
	public void Select_And_Exclude_StillWorkAfterTheLastStep()
	{
		var builder = QueryBuilder.ListCases().Page(0, 5).Select("title").Exclude("description");

		Json(builder).Should().Be(
			"""{"query":[{"_name":"listCase"},{"_name":"page","from":0,"to":5}],"includeFields":["title"],"excludeFields":["description"]}""");
	}
}
