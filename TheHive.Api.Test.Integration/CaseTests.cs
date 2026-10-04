using System.Net;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Comments;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Observables;
using TheHive.Api.Data.Tasks;

namespace TheHive.Api.Test.Integration;

public class CaseTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task CaseLifecycle_RoundTripsValuesAndDeletes()
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
					Tags = ["integration-test"],
					Summary = "initial summary"
				},
				CancellationToken);
			caseId = created.Id;
			created.Id.Should().NotBeNullOrEmpty();

			var fetched = await client.Cases.GetAsync(caseId, CancellationToken);
			fetched.Title.Should().Be(title);
			fetched.Severity.Should().Be(Severity.Low);
			fetched.Tags.Should().Contain("integration-test");
			fetched.Summary.Should().Be("initial summary");

			await client.Cases.UpdateAsync(
				caseId,
				new CaseUpdateRequest
				{
					Title = title + " (updated)",
					Severity = Severity.High,
					Flag = true,
					AddTags = ["integration-test-updated"],
					Summary = new Optional<string?>(null)
				},
				CancellationToken);

			var updated = await client.Cases.GetAsync(caseId, CancellationToken);
			updated.Title.Should().Be(title + " (updated)");
			updated.Severity.Should().Be(Severity.High);
			updated.Flag.Should().BeTrue();
			updated.Tags.Should().Contain(["integration-test", "integration-test-updated"]);
			updated.Summary.Should().BeNull();

			await client.Cases.DeleteAsync(caseId, CancellationToken);
			var deletedId = caseId;
			caseId = null;

			var act = () => client.Cases.GetAsync(deletedId, CancellationToken);
			(await act.Should().ThrowAsync<TheHiveApiException>())
				.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
	public async Task Case_WithTaskObservableAndComment_RoundTrips()
	{
		var client = Client;
		string? caseId = null;
		try
		{
			var created = await client.Cases.CreateAsync(
				new CaseCreateRequest
				{
					Title = NewName(),
					Description = "Created by the TheHive.Api integration tests; safe to delete."
				},
				CancellationToken);
			caseId = created.Id;

			var task = await client.Tasks.CreateAsync(
				caseId,
				new CaseTaskCreateRequest { Title = "integration task", Group = "integration", Description = "task description" },
				CancellationToken);
			var fetchedTask = await client.Tasks.GetAsync(task.Id, CancellationToken);
			fetchedTask.Title.Should().Be("integration task");
			fetchedTask.Description.Should().Be("task description");

			var data = $"{Guid.NewGuid():N}.example.com";
			var observables = await client.Observables.CreateInCaseAsync(
				caseId,
				new ObservableInput { DataType = "domain", Data = [data], Message = "integration observable" },
				new(), CancellationToken);
			observables.Should().ContainSingle();
			var fetchedObservable = await client.Observables.GetAsync(observables[0].Id, CancellationToken);
			fetchedObservable.DataType.Should().Be("domain");
			fetchedObservable.Data.Should().Be(data);

			var comment = await client.Comments.AddToCaseAsync(
				caseId,
				new CommentRequest { Message = "integration comment" },
				CancellationToken);
			comment.Message.Should().Be("integration comment");
		}
		finally
		{
			if (caseId is not null)
			{
				await TryCleanupAsync(() => client.Cases.DeleteAsync(caseId, CancellationToken.None));
			}
		}
	}
}
