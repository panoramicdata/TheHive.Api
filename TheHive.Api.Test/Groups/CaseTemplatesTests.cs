using System.Net;
using TheHive.Api.Data.CaseTemplates;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Pages;
using TheHive.Api.Data.Tasks;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CaseTemplatesTests
{
	private const string FullTemplateJson = """
		{
			"_id":"~84123","_type":"caseTemplate","_createdBy":"alice@example.com","_updatedBy":"lucas@example.com",
			"_createdAt":1748476800000,"_updatedAt":1748563200000,
			"name":"Ransomware Response","displayName":"Ransomware Response","titlePrefix":"RAN-",
			"description":"Ransomware infection detected.","severity":3,"severityLabel":"HIGH",
			"tags":["ransomware","TheOrganization"],"flag":true,"tlp":2,"tlpLabel":"AMBER","pap":1,"papLabel":"GREEN",
			"summary":"Opened after file encryption was detected.",
			"customFields":[{"_id":"~123456789","name":"threat-type","type":"string","value":"Malware","order":0}],
			"tasks":[{
				"_id":"~84124","_type":"Task","_createdBy":"alice@example.com","_createdAt":1748476800000,
				"title":"Isolate affected workstation","group":"Containment","status":"Waiting","flag":false,
				"order":1,"mandatory":false,"extraData":{}
			}],
			"extraData":{"usage":4}
		}
		""";

	private const string MinimalTemplateJson = """
		{
			"_id":"~1","_type":"caseTemplate","_createdBy":"alice@example.com","_createdAt":1748476800000,
			"name":"n","displayName":"n","flag":false,"extraData":{}
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullTemplate(CaseTemplate item)
	{
		item.Id.Should().Be("~84123");
		item.Type.Should().Be("caseTemplate");
		item.CreatedBy.Should().Be("alice@example.com");
		item.UpdatedBy.Should().Be("lucas@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748476800000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1748563200000));
		item.Name.Should().Be("Ransomware Response");
		item.DisplayName.Should().Be("Ransomware Response");
		item.TitlePrefix.Should().Be("RAN-");
		item.Description.Should().Be("Ransomware infection detected.");
		item.Severity.Should().Be(Severity.High);
		item.SeverityLabel.Should().Be("HIGH");
		item.Tags.Should().Equal("ransomware", "TheOrganization");
		item.Flag.Should().BeTrue();
		item.Tlp.Should().Be(Tlp.Amber);
		item.TlpLabel.Should().Be("AMBER");
		item.Pap.Should().Be(Pap.Green);
		item.PapLabel.Should().Be("GREEN");
		item.Summary.Should().Be("Opened after file encryption was detected.");
		item.CustomFields.Should().ContainSingle().Which.Should().Match<CustomFieldValue>(
			v => v.Id == "~123456789" && v.Name == "threat-type" && v.Type == "string" && v.Value.GetString() == "Malware" && v.Order == 0);
		item.Tasks.Should().ContainSingle().Which.Should().Match<CaseTask>(
			t => t.Id == "~84124" && t.Title == "Isolate affected workstation" && t.Group == "Containment" && t.Status == CaseTaskStatus.Waiting);
		item.ExtraData["usage"].GetInt32().Should().Be(4);
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, FullTemplateJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseTemplates.CreateAsync(
			new CaseTemplateCreateRequest
			{
				Name = "Ransomware Response",
				DisplayName = "Ransomware Response",
				TitlePrefix = "RAN-",
				Description = "Ransomware infection detected.",
				Severity = Severity.High,
				Tags = ["ransomware", "TheOrganization"],
				Flag = true,
				Tlp = Tlp.Amber,
				Pap = Pap.Green,
				Summary = "Opened after file encryption was detected.",
				Tasks = [new CaseTaskCreateRequest { Title = "Isolate affected workstation", Group = "Containment", Status = CaseTaskStatus.Waiting, Order = 1 }],
				CustomFields = [new CustomFieldInput { Name = "threat-type", Value = "Malware", Order = 0 }],
				PageTemplates = [new PageCreateRequest { Title = "Investigation Notes", Content = "Notes", Order = 0, Category = "Investigation" }]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseTemplate");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"Ransomware Response","displayName":"Ransomware Response","titlePrefix":"RAN-","description":"Ransomware infection detected.","severity":3,"tags":["ransomware","TheOrganization"],"flag":true,"tlp":2,"pap":1,"summary":"Opened after file encryption was detected.","tasks":[{"title":"Isolate affected workstation","group":"Containment","status":"Waiting","order":1}],"customFields":[{"name":"threat-type","value":"Malware","order":0}],"pageTemplates":[{"title":"Investigation Notes","content":"Notes","order":0,"category":"Investigation"}]}""");
		AssertFullTemplate(result);
	}

	[Fact]
	public async Task CreateAsync_NameOnly_OmitsOptionals()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalTemplateJson);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.CreateAsync(new CaseTemplateCreateRequest { Name = "n" }, TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"name":"n"}""");
	}

	[Fact]
	public async Task GetAsync_GetsByName_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.OK, MinimalTemplateJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseTemplates.GetAsync("my template", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseTemplate/my%20template");
		stub.Calls[0].Body.Should().BeNull();
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.TitlePrefix.Should().BeNull();
		result.Description.Should().BeNull();
		result.Severity.Should().BeNull();
		result.SeverityLabel.Should().BeNull();
		result.Tlp.Should().BeNull();
		result.TlpLabel.Should().BeNull();
		result.Pap.Should().BeNull();
		result.PapLabel.Should().BeNull();
		result.Summary.Should().BeNull();
		result.Tags.Should().BeEmpty();
		result.CustomFields.Should().BeEmpty();
		result.Tasks.Should().BeEmpty();
	}

	[Fact]
	public async Task GetAsync_FullTemplate_MapsEveryField()
	{
		var stub = Stub(HttpStatusCode.OK, FullTemplateJson);
		using var client = TestClient.Create(stub);

		AssertFullTemplate(await client.CaseTemplates.GetAsync("~84123", TestContext.Current.CancellationToken));
	}

	[Fact]
	public void CaseTemplate_Defaults_AreEmptyNotNull()
	{
		var item = new CaseTemplate();

		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Name.Should().BeEmpty();
		item.DisplayName.Should().BeEmpty();
		item.ExtraData.Should().BeEmpty();
	}

	[Fact]
	public async Task UpdateAsync_SendsSetValuesAndExplicitNulls()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.UpdateAsync(
			"~84123",
			new CaseTemplateUpdateRequest
			{
				Name = "New name",
				DisplayName = "New display",
				TitlePrefix = null,
				Description = null,
				Severity = null,
				Tags = ["a"],
				Flag = true,
				Tlp = null,
				Pap = null,
				Summary = null,
				CustomFields = [new CustomFieldInput { Name = "threat-type", Value = 3 }],
				Tasks = [new CaseTaskCreateRequest { Title = "t" }]
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseTemplate/~84123");
		stub.Calls[0].Body.Should().Be(
			"""{"name":"New name","displayName":"New display","titlePrefix":null,"description":null,"severity":null,"tags":["a"],"flag":true,"tlp":null,"pap":null,"summary":null,"customFields":[{"name":"threat-type","value":3}],"tasks":[{"title":"t"}]}""");
	}

	[Fact]
	public async Task UpdateAsync_SetsValues_And_UnsetOptionalsAreOmitted()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.UpdateAsync(
			"~84123",
			new CaseTemplateUpdateRequest { TitlePrefix = "RAN-", Description = "d", Severity = 2, Tlp = 1, Pap = 0, Summary = "s" },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"titlePrefix":"RAN-","description":"d","severity":2,"tlp":1,"pap":0,"summary":"s"}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.UpdateAsync("~84123", new CaseTemplateUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.DeleteAsync("~84123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseTemplate/~84123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task LinkPageTemplatesAsync_PutsIds()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.LinkPageTemplatesAsync(
			"Ransomware Response",
			new CaseTemplatePageLinkRequest { PageTemplateIds = ["~84123", "~84456"] },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Put);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseTemplate/Ransomware%20Response/pageTemplate/link");
		stub.Calls[0].Body.Should().Be("""{"pageTemplateIds":["~84123","~84456"]}""");
	}

	[Fact]
	public async Task LinkPageTemplatesAsync_EmptyList_RemovesAllLinks_And_NullIsOmitted()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent);
		stub.Enqueue(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseTemplates.LinkPageTemplatesAsync("~1", new CaseTemplatePageLinkRequest { PageTemplateIds = [] }, TestContext.Current.CancellationToken);
		await client.CaseTemplates.LinkPageTemplatesAsync("~1", new CaseTemplatePageLinkRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"pageTemplateIds":[]}""");
		stub.Calls[1].Body.Should().Be("{}");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"CaseTemplate not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.CaseTemplates.GetAsync("missing", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
