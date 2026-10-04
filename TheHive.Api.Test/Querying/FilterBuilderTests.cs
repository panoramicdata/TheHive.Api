using System.Text.Json;
using TheHive.Api.Querying;

namespace TheHive.Api.Test.Querying;

public class FilterBuilderTests
{
	private static string FilterStep(Action<FilterBuilder> filter)
	{
		var request = QueryBuilder.ListCases().Filter(filter).Build();
		return JsonSerializer.Serialize(request.Query[1], TheHiveJson.Options);
	}

	[Theory]
	[InlineData("Eq", "_eq")]
	[InlineData("Ne", "_ne")]
	[InlineData("Lt", "_lt")]
	[InlineData("Lte", "_lte")]
	[InlineData("Gt", "_gt")]
	[InlineData("Gte", "_gte")]
	public void Comparisons_WriteFieldAndValue(string method, string wire)
	{
		var result = FilterStep(f => typeof(FilterBuilder).GetMethod(method)!.Invoke(f, ["severity", 3]));

		result.Should().Be($$$"""{"_name":"filter","{{{wire}}}":{"_field":"severity","_value":3}}""");
	}

	[Theory]
	[InlineData("Like", "_like")]
	[InlineData("Match", "_match")]
	[InlineData("StartsWith", "_startsWith")]
	[InlineData("EndsWith", "_endsWith")]
	public void TextFilters_WriteFieldAndValue(string method, string wire)
	{
		var result = FilterStep(f => typeof(FilterBuilder).GetMethod(method)!.Invoke(f, ["title", "Ransom"]));

		result.Should().Be($$$"""{"_name":"filter","{{{wire}}}":{"_field":"title","_value":"Ransom"}}""");
	}

	[Fact]
	public void Between_WritesFromAndTo()
	{
		FilterStep(f => f.Between("severity", 1, 3))
			.Should().Be("""{"_name":"filter","_between":{"_field":"severity","_from":1,"_to":3}}""");
	}

	[Fact]
	public void In_WritesValues()
	{
		FilterStep(f => f.In("status", "New", "InProgress"))
			.Should().Be("""{"_name":"filter","_in":{"_field":"status","_values":["New","InProgress"]}}""");
	}

	[Fact]
	public void Has_Id_And_Any_WriteTheirOperand()
	{
		FilterStep(f => f.Has("assignee")).Should().Be("""{"_name":"filter","_has":"assignee"}""");
		FilterStep(f => f.Id("~123")).Should().Be("""{"_name":"filter","_id":"~123"}""");
		FilterStep(f => f.Any()).Should().Be("""{"_name":"filter","_any":null}""");
	}

	[Fact]
	public void NestedGroups_ComposeRecursively()
	{
		var result = FilterStep(f => f
			.Eq("status", "New")
			.Or(o => o.Like("title", "phish").And(a => a.Gte("severity", 3).Not(n => n.Has("assignee")))));

		result.Should().Be(
			"""{"_name":"filter","_and":[{"_eq":{"_field":"status","_value":"New"}},""" +
			"""{"_or":[{"_like":{"_field":"title","_value":"phish"}},{"_and":[{"_gte":{"_field":"severity","_value":3}},{"_not":{"_has":"assignee"}}]}]}]}""");
	}

	[Fact]
	public void Not_WithSeveralConditions_NegatesTheirConjunction()
	{
		FilterStep(f => f.Not(n => n.Eq("a", 1).Eq("b", 2)))
			.Should().Be("""{"_name":"filter","_not":{"_and":[{"_eq":{"_field":"a","_value":1}},{"_eq":{"_field":"b","_value":2}}]}}""");
	}

	[Fact]
	public void In_NoValues_Throws()
	{
		var act = () => FilterStep(f => f.In("status"));

		act.Should().Throw<ArgumentException>().WithParameterName("values");
	}

	[Fact]
	public void In_NullValues_Throws()
	{
		var act = () => FilterStep(f => f.In("status", null!));

		act.Should().Throw<ArgumentNullException>().WithParameterName("values");
	}

	[Fact]
	public void SingleOperandFilters_BlankOperand_Throw()
	{
		var has = () => FilterStep(f => f.Has(" "));
		var id = () => FilterStep(f => f.Id(""));

		has.Should().Throw<ArgumentException>().WithParameterName("field");
		id.Should().Throw<ArgumentException>().WithParameterName("entityId");
	}

	[Fact]
	public void Between_BlankField_Throws()
	{
		var act = () => FilterStep(f => f.Between("", 1, 2));

		act.Should().Throw<ArgumentException>().WithParameterName("field");
	}

	[Fact]
	public void EmptyGroups_Throw()
	{
		var and = () => FilterStep(f => f.And(_ => { }));
		var or = () => FilterStep(f => f.Or(_ => { }));
		var not = () => FilterStep(f => f.Not(_ => { }));

		and.Should().Throw<ArgumentException>().WithMessage("*at least one condition*");
		or.Should().Throw<ArgumentException>().WithMessage("*at least one condition*");
		not.Should().Throw<ArgumentException>().WithMessage("*at least one condition*");
	}

	[Fact]
	public void NullGroupAction_Throws()
	{
		var act = () => FilterStep(f => f.And(null!));

		act.Should().Throw<ArgumentNullException>();
	}
}
