using System.Net;
using TheHive.Api.Data.Alerts;
using TheHive.Api.Data.CustomFields;

namespace TheHive.Api.Test.Integration;

public class AlertAndCustomFieldTests(ITestOutputHelper testOutputHelper, Fixture fixture) : TestWithOutput(testOutputHelper, fixture)
{
	[Fact]
	public async Task AlertLifecycle_RoundTripsValuesAndDeletes()
	{
		var client = Client;
		var title = NewName();
		var sourceRef = Guid.NewGuid().ToString("N");
		string? alertId = null;
		try
		{
			var created = await client.Alerts.CreateAsync(
				new AlertCreateRequest
				{
					Type = "integration-test",
					Source = "TheHive.Api",
					SourceRef = sourceRef,
					Title = title,
					Description = "Created by the TheHive.Api integration tests; safe to delete.",
					Tags = ["integration-test"]
				},
				CancellationToken);
			alertId = created.Id;

			var fetched = await client.Alerts.GetAsync(alertId, CancellationToken);
			fetched.Title.Should().Be(title);
			fetched.SourceRef.Should().Be(sourceRef);
			fetched.Type.Should().Be("integration-test");
			fetched.Tags.Should().Contain("integration-test");

			await client.Alerts.DeleteAsync(alertId, CancellationToken);
			var deletedId = alertId;
			alertId = null;

			var act = () => client.Alerts.GetAsync(deletedId, CancellationToken);
			(await act.Should().ThrowAsync<TheHiveApiException>())
				.Which.StatusCode.Should().Be(HttpStatusCode.NotFound);
		}
		finally
		{
			if (alertId is not null)
			{
				await TryCleanupAsync(() => client.Alerts.DeleteAsync(alertId, CancellationToken.None));
			}
		}
	}

	[Fact]
	public async Task CustomFieldLifecycle_CreatesListsAndDeletes()
	{
		var client = Client;
		var name = "itest" + Guid.NewGuid().ToString("N")[..12];
		string? fieldId = null;
		try
		{
			var created = await client.CustomFields.CreateAsync(
				new CustomFieldCreateRequest
				{
					Name = name,
					DisplayName = name,
					Group = "integration",
					Description = "Created by the TheHive.Api integration tests; safe to delete.",
					Type = CustomFieldType.String
				},
				CancellationToken);
			fieldId = created.Id;

			var listed = await client.CustomFields.ListAsync(CancellationToken);
			var found = listed.Should().ContainSingle(f => f.Id == fieldId).Which;
			found.Name.Should().Be(name);
			found.Type.Should().Be(CustomFieldType.String);

			await client.CustomFields.DeleteAsync(fieldId, cancellationToken: CancellationToken);
			var deletedId = fieldId;
			fieldId = null;

			var after = await client.CustomFields.ListAsync(CancellationToken);
			after.Should().NotContain(f => f.Id == deletedId);
		}
		finally
		{
			if (fieldId is not null)
			{
				await TryCleanupAsync(() => client.CustomFields.DeleteAsync(fieldId, force: true, cancellationToken: CancellationToken.None));
			}
		}
	}
}
