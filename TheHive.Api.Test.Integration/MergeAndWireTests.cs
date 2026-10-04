using System.Net;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Shares;

namespace TheHive.Api.Test.Integration;

/// <summary>Live checks of request shapes that depend on how Refit builds the request: a list in one path segment, a DELETE with a JSON body and a raw text response.</summary>
public class MergeAndWireTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task MergeCases_CommaSeparatedIds_CreatesOneCaseAndDeletesTheSources()
	{
		var client = Client;
		var createdIds = new List<string>();
		try
		{
			var first = await client.Cases.CreateAsync(NewCase(), CancellationToken);
			createdIds.Add(first.Id);
			var second = await client.Cases.CreateAsync(NewCase(), CancellationToken);
			createdIds.Add(second.Id);

			// The comma travels percent-encoded (%2C) in the one path segment; the server accepts it.
			var merged = await client.Cases.MergeAsync($"{first.Id},{second.Id}", CancellationToken);
			createdIds.Add(merged.Id);

			merged.Id.Should().NotBe(first.Id).And.NotBe(second.Id);
			merged.Title.Should().Be($"{first.Title} / {second.Title}");
			merged.Description.Should().Contain(first.Title).And.Contain(second.Title);

			foreach (var sourceId in new[] { first.Id, second.Id })
			{
				var getSource = () => client.Cases.GetAsync(sourceId, CancellationToken);
				(await getSource.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
			}

			(await client.Cases.GetAsync(merged.Id, CancellationToken)).Title.Should().Be(merged.Title);

			await client.Cases.DeleteAsync(merged.Id, CancellationToken);
			var getMerged = () => client.Cases.GetAsync(merged.Id, CancellationToken);
			(await getMerged.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}
		finally
		{
			foreach (var id in createdIds)
			{
				await TryCleanupAsync(() => client.Cases.DeleteAsync(id, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task UnshareCase_SendsTheJsonBodyWithDelete()
	{
		var client = Client;
		string? caseId = null;
		try
		{
			caseId = (await client.Cases.CreateAsync(NewCase(), CancellationToken)).Id;
			(await client.Shares.ListByCaseAsync(caseId, CancellationToken)).Should().BeEmpty();

			// The server reads the body: an empty list is a no-op (204, a Task return) ...
			await client.Shares.UnshareCaseAsync(caseId, new ShareRemoveRequest { Organisations = [] }, CancellationToken);

			// ... an unknown organization is looked up and not found ...
			var unknownOrganisation = () => client.Shares.UnshareCaseAsync(
				caseId,
				new ShareRemoveRequest { Organisations = [$"itest-{Guid.NewGuid():N}"] },
				CancellationToken);
			(await unknownOrganisation.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.NotFound);

			// ... and a body without "organisations" is rejected as invalid.
			var missingField = () => client.Shares.UnshareCaseAsync(caseId, new ShareRemoveRequest(), CancellationToken);
			(await missingField.Should().ThrowAsync<TheHiveApiException>()).Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);

			(await client.Shares.ListByCaseAsync(caseId, CancellationToken)).Should().BeEmpty();
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
	public async Task GetLicenseChallenge_ReturnsTheRawTextToken()
	{
		// Read-only; needs managePlatform, so it runs in the built-in admin organisation. The token is not printed.
		using var admin = CreateClientFor("admin");

		var challenge = await admin.License.GetChallengeAsync(CancellationToken);

		challenge.Should().NotBeNullOrWhiteSpace();
		challenge.Should().NotStartWith("\"").And.NotStartWith("{");
	}

	private static CaseCreateRequest NewCase() => new()
	{
		Title = NewName(),
		Description = "Created by the TheHive.Api integration tests; safe to delete."
	};
}
