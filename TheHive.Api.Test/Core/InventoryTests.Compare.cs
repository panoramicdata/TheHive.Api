using Refit;
using System.Reflection;
using TheHive.Api.Interfaces;

namespace TheHive.Api.Test.Core;

public partial class InventoryTests
{
	[Fact]
	public void CompareRow_WrapperWithItsRefitTwin_ReportsNothing() =>
		CompareRow(
			new Row(1, "Branding", "POST", "`/api/v1/branding`", "", ""),
			[typeof(IBranding).GetMethod(nameof(IBranding.SetAsync))!, typeof(IBranding).GetMethod(nameof(IBranding.SetMultipartAsync))!])
			.Should().BeEmpty();

	[Fact]
	public void CompareRow_WrapperWithoutATwin_ReportsTheWrapper()
	{
		var row = new Row(9, "Branding", "POST", "`/api/v1/branding`", "", "");

		var messages = CompareRow(row, [typeof(IBranding).GetMethod(nameof(IBranding.SetAsync))!]);

		messages.Should().Equal("line 9: Branding POST `/api/v1/branding`: IBranding.SetAsync has no Refit HTTP method attribute");
	}

	[Fact]
	public void CompareRow_WrapperWithAWrongTwin_ReportsTheTwin()
	{
		var row = new Row(9, "Branding", "GET", "`/api/v1/branding`", "", "");

		var messages = CompareRow(row, [typeof(IBranding).GetMethod(nameof(IBranding.SetAsync))!, typeof(IBranding).GetMethod(nameof(IBranding.SetMultipartAsync))!]);

		messages.Should().Equal("line 9: Branding GET `/api/v1/branding`: IBranding.SetMultipartAsync is POST api/v1/branding, expected GET api/v1/branding");
	}

	[Fact]
	public void CompareRow_AbstractMethodWithoutRefitAttribute_IsNotAWrapper()
	{
		var row = new Row(9, "Case", "GET", "`/api/v1/case`", "", "");

		var messages = CompareRow(row, [typeof(AbstractBesideRefit).GetMethod(nameof(AbstractBesideRefit.GetAsync))!, typeof(AbstractBesideRefit).GetMethod(nameof(AbstractBesideRefit.Unattributed))!]);

		messages.Should().Equal("line 9: Case GET `/api/v1/case`: AbstractBesideRefit.Unattributed has no Refit HTTP method attribute");
	}

	/// <summary>
	/// An abstract method without a Refit attribute next to a Refit-attributed method of the same type: not a wrapper, so it must be reported.
	/// An abstract class stands in for an interface because Refit's source generator rejects (RF001) an interface mixing the two.
	/// </summary>
	public abstract class AbstractBesideRefit
	{
		/// <summary>A Refit-attributed method matching the row.</summary>
		/// <returns>Nothing.</returns>
		[Get("api/v1/case")]
		public abstract Task GetAsync();

		/// <summary>An abstract method with no Refit attribute and no body.</summary>
		public abstract void Unattributed();
	}

	[Fact]
	public void CompareRoute_MatchingVerbAndPath_ReturnsNull() =>
		CompareRoute(
			new Row(1, "Case", "POST", "`/api/v1/case/_merge/{ids}` (verify: a note)", "", ""),
			typeof(ICases).GetMethod(nameof(ICases.MergeAsync))!)
			.Should().BeNull();

	[Fact]
	public void CompareRoute_WrongVerb_ReportsTheRow()
	{
		var row = new Row(9, "Case", "GET", "`/api/v1/case/{idOrName}`", "", "");

		var message = CompareRoute(row, typeof(ICases).GetMethod(nameof(ICases.DeleteAsync))!);

		message.Should().Be("line 9: Case GET `/api/v1/case/{idOrName}`: ICases.DeleteAsync is DELETE api/v1/case/{idOrName}, expected GET api/v1/case/{idOrName}");
	}

	[Fact]
	public void CompareRoute_WrongPath_ReportsTheRow()
	{
		var row = new Row(9, "Case", "GET", "`/api/v1/case/{caseId}`", "", "");

		var message = CompareRoute(row, typeof(ICases).GetMethod(nameof(ICases.GetAsync))!);

		message.Should().Be("line 9: Case GET `/api/v1/case/{caseId}`: ICases.GetAsync is GET api/v1/case/{idOrName}, expected GET api/v1/case/{caseId}");
	}

	[Fact]
	public void CompareRoute_NoRefitAttribute_ReportsTheRow()
	{
		var row = new Row(9, "Case", "GET", "`/api/v1/case`", "", "");

		var message = CompareRoute(row, typeof(object).GetMethod(nameof(ToString))!);

		message.Should().Be("line 9: Case GET `/api/v1/case`: Object.ToString has no Refit HTTP method attribute");
	}

	[Fact]
	public void PendingTags_AreNotFullyImplemented()
	{
		var finished = PendingTags.Where(tag => Rows.Where(r => r.Group == tag && !r.IsDeprecated).All(r => r.IsComplete));

		AssertNone(finished, "a tag whose non-deprecated rows are all filled must be removed from docs/pending-tags.txt");
	}

	/// <summary>
	/// Checks every method a row names. A method with a Refit attribute is compared with <see cref="CompareRoute"/>. A method without one is
	/// accepted only when it is a default-implemented interface method (a wrapper that takes a request object, such as <c>IBranding.SetAsync</c>)
	/// and the same row also names a Refit-attributed method of the same interface (its raw twin, such as
	/// <c>IBranding.SetMultipartAsync</c>), which is then compared as usual; anything else is reported.
	/// </summary>
	/// <param name="row">The inventory row.</param>
	/// <param name="methods">The interface methods the row names.</param>
	/// <returns>One message per mismatch; empty when they all match.</returns>
	public static List<string> CompareRow(Row row, IReadOnlyList<MethodInfo> methods)
	{
		var messages = new List<string>();
		foreach (var method in methods)
		{
			var isWrapper = method.GetCustomAttribute<HttpMethodAttribute>() is null
				&& !method.IsAbstract
				&& methods.Any(m => m.DeclaringType == method.DeclaringType && m.GetCustomAttribute<HttpMethodAttribute>() is not null);
			if (!isWrapper && CompareRoute(row, method) is { } message)
			{
				messages.Add(message);
			}
		}

		return messages;
	}

	/// <summary>Compares a row's verb and path (the first backticked token of the Path cell, notes ignored) with the Refit attribute on <paramref name="method"/>.</summary>
	/// <param name="row">The inventory row.</param>
	/// <param name="method">The interface method the row names.</param>
	/// <returns><see langword="null"/> when they match; otherwise a message naming the row.</returns>
	public static string? CompareRoute(Row row, MethodInfo method)
	{
		var name = $"{method.DeclaringType!.Name}.{method.Name}";
		var attribute = method.GetCustomAttribute<HttpMethodAttribute>();
		if (attribute is null)
		{
			return $"{row}: {name} has no Refit HTTP method attribute";
		}

		var expectedPath = Tokens(row.Path).FirstOrDefault()?.TrimStart('/') ?? string.Empty;
		var actualVerb = attribute.Method.Method;
		return actualVerb == row.Method && attribute.Path == expectedPath
			? null
			: $"{row}: {name} is {actualVerb} {attribute.Path}, expected {row.Method} {expectedPath}";
	}
}
