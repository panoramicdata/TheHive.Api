using System.Reflection;
using System.Text.RegularExpressions;
using Refit;
using TheHive.Api.Interfaces;

namespace TheHive.Api.Test.Core;

/// <summary>Keeps docs/endpoint-coverage.md and the public interfaces in step.</summary>
public partial class InventoryTests
{
	private const string Header = "| Group | Method | Path | Client method | Test |";
	private const string InterfaceNamespace = "TheHive.Api.Interfaces";

	private static readonly string RepoRoot = FindRepoRoot();
	private static readonly IReadOnlyList<Row> Rows = ParseRows();
	private static readonly IReadOnlySet<string> PendingTags = ReadPendingTags();

	public sealed record Row(int Line, string Group, string Method, string Path, string ClientMethod, string Test)
	{
		public bool IsDeprecated => Path.Contains("(deprecated)", StringComparison.Ordinal);

		public bool IsComplete => ClientMethod.Length > 0 && Test.Length > 0;

		public bool IsStarted => ClientMethod.Length > 0 || Test.Length > 0;

		public override string ToString() => $"line {Line}: {Group} {Method} {Path}";
	}

	[Fact]
	public void Table_HasEveryInventoriedOperation() =>
		Rows.Should().HaveCount(272, "docs/endpoint-coverage.md inventories 272 operations");

	[Fact]
	public void PendingTags_AreTagsInTheTable()
	{
		var tags = Rows.Select(r => r.Group).ToHashSet(StringComparer.Ordinal);

		AssertNone(PendingTags.Where(t => !tags.Contains(t)), "every line of docs/pending-tags.txt must name a Group in the table");
	}

	[Fact]
	public void NonPendingTags_HaveEveryRowImplemented()
	{
		var offending = Rows
			.Where(r => !r.IsDeprecated && !PendingTags.Contains(r.Group) && !r.IsComplete)
			.Select(r => r.ToString());

		AssertNone(offending, "a group removed from docs/pending-tags.txt must fill Client method and Test on every non-deprecated row");
	}

	[Fact]
	public void StartedRows_HaveBothClientMethodAndTest()
	{
		var offending = Rows.Where(r => r.IsStarted && !r.IsComplete).Select(r => r.ToString());

		AssertNone(offending, "a row must fill both Client method and Test, or neither");
	}

	[Fact]
	public void DeprecatedRows_AreNotImplemented()
	{
		var offending = Rows.Where(r => r.IsDeprecated && r.IsStarted).Select(r => r.ToString());

		AssertNone(offending, "deprecated operations are never implemented");
	}

	[Fact]
	public void ClientMethods_ExistOnTheNamedInterface()
	{
		var assembly = typeof(TheHiveClient).Assembly;
		var offending = new List<string>();
		foreach (var row in Rows.Where(r => r.ClientMethod.Length > 0))
		{
			var references = Tokens(row.ClientMethod);
			if (references.Count == 0)
			{
				offending.Add($"{row}: no `IGroup.MethodAsync` reference in '{row.ClientMethod}'");
			}

			foreach (var reference in references)
			{
				var (typeName, methodName) = Split(reference);
				var type = assembly.GetType($"{InterfaceNamespace}.{typeName}");
				if (type is null || !type.IsInterface || type.GetMethods().All(m => m.Name != methodName))
				{
					offending.Add($"{row}: {reference} not found");
				}
			}
		}

		AssertNone(offending, "each reference must resolve");
	}

	[Fact]
	public void ClientMethods_MatchTheRowVerbAndPath()
	{
		var assembly = typeof(TheHiveClient).Assembly;
		var offending = new List<string>();
		foreach (var row in Rows.Where(r => r.ClientMethod.Length > 0))
		{
			foreach (var (typeName, methodName) in Tokens(row.ClientMethod).Select(Split))
			{
				var methods = assembly.GetType($"{InterfaceNamespace}.{typeName}")?.GetMethods().Where(m => m.Name == methodName) ?? [];
				offending.AddRange(methods.Select(m => CompareRoute(row, m)).OfType<string>());
			}
		}

		AssertNone(offending, "each Client method's Refit verb and path must match its row");
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

	[Fact]
	public void Tests_ExistInTheTestAssembly()
	{
		var types = typeof(InventoryTests).Assembly.GetTypes();
		var offending = new List<string>();
		foreach (var row in Rows.Where(r => r.Test.Length > 0))
		{
			var references = Tokens(row.Test);
			if (references.Count == 0)
			{
				offending.Add($"{row}: no `Class.Method` reference in '{row.Test}'");
			}

			foreach (var reference in references)
			{
				var (typeName, methodName) = Split(reference);
				var found = types
					.Where(t => t.Name == typeName)
					.SelectMany(t => t.GetMethods())
					.Any(m => m.Name == methodName && m.GetCustomAttribute<FactAttribute>() is not null);
				if (!found)
				{
					offending.Add($"{row}: test {reference} not found");
				}
			}
		}

		AssertNone(offending, "each reference must resolve");
	}

	[Fact]
	public void EveryInterfaceMethod_IsListed()
	{
		var listed = Rows.SelectMany(r => Tokens(r.ClientMethod)).ToHashSet(StringComparer.Ordinal);
		var interfaces = typeof(TheHiveClient).Assembly.GetExportedTypes()
			.Where(t => t.IsInterface && t.Namespace == InterfaceNamespace);

		var missing = interfaces
			.SelectMany(t => t.GetMethods().Select(m => $"{t.Name}.{m.Name}"))
			.Where(name => !listed.Contains(name));

		AssertNone(missing, "every interface method must appear in a Client method cell of docs/endpoint-coverage.md");
	}

	private static void AssertNone(IEnumerable<string> offending, string because)
	{
		var list = offending.ToList();
		list.Should().BeEmpty("{0}. Offending ({1}):{2}{3}", because, list.Count, Environment.NewLine, string.Join(Environment.NewLine, list));
	}

	private static List<string> Tokens(string cell) => [.. BacktickToken().Matches(cell).Select(m => m.Groups[1].Value)];

	private static (string Type, string Method) Split(string reference)
	{
		var dot = reference.LastIndexOf('.');
		return dot < 0 ? (reference, string.Empty) : (reference[..dot], reference[(dot + 1)..]);
	}

	private static string FindRepoRoot()
	{
		var directory = new DirectoryInfo(AppContext.BaseDirectory);
		while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "TheHive.Api.slnx")))
		{
			directory = directory.Parent;
		}

		return directory?.FullName ?? throw new InvalidOperationException($"TheHive.Api.slnx not found above {AppContext.BaseDirectory}.");
	}

	private static List<Row> ParseRows()
	{
		var lines = File.ReadAllLines(Path.Combine(RepoRoot, "docs", "endpoint-coverage.md"));
		var start = Array.FindIndex(lines, l => l.Trim() == Header);
		if (start < 0)
		{
			throw new InvalidOperationException($"Operation table header '{Header}' not found in docs/endpoint-coverage.md.");
		}

		var rows = new List<Row>();
		for (var i = start + 2; i < lines.Length && lines[i].StartsWith('|'); i++)
		{
			var cells = lines[i].Split('|');
			if (cells.Length != 7)
			{
				throw new InvalidOperationException($"docs/endpoint-coverage.md line {i + 1} has {cells.Length - 2} cells, expected 5: {lines[i]}");
			}

			rows.Add(new Row(i + 1, cells[1].Trim(), cells[2].Trim(), cells[3].Trim(), cells[4].Trim(), cells[5].Trim()));
		}

		return rows;
	}

	private static HashSet<string> ReadPendingTags() =>
		File.ReadAllLines(Path.Combine(RepoRoot, "docs", "pending-tags.txt"))
			.Select(l => l.Trim())
			.Where(l => l.Length > 0 && !l.StartsWith('#'))
			.ToHashSet(StringComparer.Ordinal);

	[GeneratedRegex("`([^`]+)`")]
	private static partial Regex BacktickToken();
}
