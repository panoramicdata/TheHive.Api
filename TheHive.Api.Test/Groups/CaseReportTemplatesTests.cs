using System.Net;
using System.Text.Json;
using Refit;
using TheHive.Api.Data.CaseReportTemplates;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CaseReportTemplatesTests
{
	private const string DefinitionJson = """
		{
			"widgets":[
				{"_kind":"Text","title":"Summary","template":"Case {{case.title}}"},
				{"_kind":"Image","title":"Company Logo","attachmentId":"~456789012"},
				{"_kind":"ObservableTable","title":"Observables","columns":["dataType","data","tlp"],"filter":{"_eq":{"_field":"ioc","_value":true}},"sort":["+_createdAt"],"protectData":true,"maxElements":20},
				{"_kind":"TaskList","fields":["title","status"],"logColumns":["message"],"withTaskLogs":false},
				{"_kind":"Timeline","events":["Alert","Case"],"withCustomEventsDescription":true},
				{"_kind":"FutureWidget","novelty":{"level":3}}
			],
			"header":{"template":"Incident Report - {{case.title}}"},
			"footer":{"template":"Confidential - {{case.title}}"},
			"dateFormat":"yyyy-MM-dd","dateTimeFormat":"yyyy-MM-dd HH:mm","i18n":{"lang":"en"}
		}
		""";

	private static readonly string TemplateJson = $$"""
		{
			"_id":"~42123","_type":"caseReportTemplate","_createdBy":"lucas@example.com","_updatedBy":"alice@example.com",
			"_createdAt":1718532000000,"_updatedAt":1718618400000,"title":"Ransomware Investigation Report",
			"group":"Incident Response","description":"Standard template for ransomware incident investigations.",
			"version":1,"definition":{{DefinitionJson}}
		}
		""";

	private const string MinimalTemplateJson = """
		{
			"_id":"~1","_type":"caseReportTemplate","_createdBy":"lucas@example.com","_createdAt":1718532000000,
			"title":"t","group":"g","description":"d","version":1,"definition":{"i18n":{}}
		}
		""";

	private const string AttachmentJson = """
		{
			"_id":"~456789012","_type":"Attachment","_createdBy":"lucas@example.com","_createdAt":1718532000000,
			"name":"logo.png","hashes":["fake-hash-0001"],"size":2048,"contentType":"image/png",
			"id":"a1b2c3d4","path":"/data/a1b2","extraData":{},"external":false
		}
		""";

	private static StubHandler Stub(HttpStatusCode status, string json = "")
	{
		var stub = new StubHandler();
		stub.Enqueue(status, json);
		return stub;
	}

	private static void AssertFullTemplate(CaseReportTemplate item)
	{
		item.Id.Should().Be("~42123");
		item.Type.Should().Be("caseReportTemplate");
		item.CreatedBy.Should().Be("lucas@example.com");
		item.UpdatedBy.Should().Be("alice@example.com");
		item.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718532000000));
		item.UpdatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1718618400000));
		item.Title.Should().Be("Ransomware Investigation Report");
		item.Group.Should().Be("Incident Response");
		item.Description.Should().Be("Standard template for ransomware incident investigations.");
		item.Version.Should().Be(1);

		var definition = item.Definition;
		definition.Header!.Template.Should().Be("Incident Report - {{case.title}}");
		definition.Footer!.Template.Should().Be("Confidential - {{case.title}}");
		definition.DateFormat.Should().Be("yyyy-MM-dd");
		definition.DateTimeFormat.Should().Be("yyyy-MM-dd HH:mm");
		definition.I18n.Lang.Should().Be("en");
		definition.Widgets.Should().HaveCount(6);

		var text = definition.Widgets![0];
		text.Kind.Should().Be(CaseReportWidgetKinds.Text);
		text.Title.Should().Be("Summary");
		text.Template.Should().Be("Case {{case.title}}");
		text.Columns.Should().BeNull();
		text.AdditionalData.Should().BeNull();

		var image = definition.Widgets[1];
		image.Kind.Should().Be(CaseReportWidgetKinds.Image);
		image.AttachmentId.Should().Be("~456789012");

		var table = definition.Widgets[2];
		table.Kind.Should().Be(CaseReportWidgetKinds.ObservableTable);
		table.Columns.Should().Equal("dataType", "data", "tlp");
		table.Filter!.Value.GetRawText().Should().Be("""{"_eq":{"_field":"ioc","_value":true}}""");
		table.Sort.Should().ContainSingle().Which.GetString().Should().Be("+_createdAt");
		table.ProtectData.Should().BeTrue();
		table.MaxElements.Should().Be(20);

		var tasks = definition.Widgets[3];
		tasks.Fields.Should().Equal("title", "status");
		tasks.LogColumns.Should().Equal("message");
		tasks.WithTaskLogs.Should().BeFalse();

		var timeline = definition.Widgets[4];
		timeline.Events.Should().Equal("Alert", "Case");
		timeline.WithCustomEventsDescription.Should().BeTrue();

		var future = definition.Widgets[5];
		future.Kind.Should().Be("FutureWidget");
		future.AdditionalData!["novelty"].GetRawText().Should().Be("""{"level":3}""");
	}

	private static CaseReportTemplateCreateRequest FullCreateRequest() => new()
	{
		Title = "Ransomware Investigation Report",
		Group = "Incident Response",
		Description = "Standard template for ransomware incident investigations.",
		Version = 1,
		Definition = new CaseReportTemplateDefinition
		{
			Widgets =
			[
				new CaseReportWidget { Kind = CaseReportWidgetKinds.Text, Title = "Summary", Template = "Case {{case.title}}" },
				new CaseReportWidget { Kind = CaseReportWidgetKinds.Image, Title = "Company Logo", AttachmentId = "~456789012" },
				new CaseReportWidget
				{
					Kind = CaseReportWidgetKinds.ObservableTable,
					Title = "Observables",
					Columns = ["dataType", "data", "tlp"],
					Filter = JsonSerializer.Deserialize<JsonElement>("""{"_eq":{"_field":"ioc","_value":true}}"""),
					Sort = [JsonSerializer.SerializeToElement("+_createdAt")],
					ProtectData = true,
					MaxElements = 20
				},
				new CaseReportWidget
				{
					Kind = CaseReportWidgetKinds.TaskList,
					Fields = ["title", "status"],
					LogColumns = ["message"],
					WithTaskLogs = false
				},
				new CaseReportWidget
				{
					Kind = CaseReportWidgetKinds.Timeline,
					Events = ["Alert", "Case"],
					WithCustomEventsDescription = true
				}
			],
			Header = new CaseReportHeader { Template = "Incident Report - {{case.title}}" },
			Footer = new CaseReportFooter { Template = "Confidential - {{case.title}}" },
			DateFormat = "yyyy-MM-dd",
			DateTimeFormat = "yyyy-MM-dd HH:mm",
			I18n = new CaseReportI18n { Lang = "en" }
		}
	};

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsEveryField()
	{
		var stub = Stub(HttpStatusCode.Created, TemplateJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseReportTemplates.CreateAsync(
			FullCreateRequest(),
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate");
		var expectedWidgets = """
			[{"_kind":"Text","title":"Summary","template":"Case {{case.title}}"},{"_kind":"Image","title":"Company Logo","attachmentId":"~456789012"},{"_kind":"ObservableTable","title":"Observables","columns":["dataType","data","tlp"],"filter":{"_eq":{"_field":"ioc","_value":true}},"sort":["PLUS_createdAt"],"protectData":true,"maxElements":20},{"_kind":"TaskList","fields":["title","status"],"logColumns":["message"],"withTaskLogs":false},{"_kind":"Timeline","events":["Alert","Case"],"withCustomEventsDescription":true}]
			""".Replace("PLUS", ((char)92) + "u002B", StringComparison.Ordinal);
		stub.Calls[0].Body.Should().Be(
			"{\"title\":\"Ransomware Investigation Report\",\"group\":\"Incident Response\",\"description\":\"Standard template for ransomware incident investigations.\",\"definition\":{\"widgets\":"
			+ expectedWidgets
			+ ",\"header\":{\"template\":\"Incident Report - {{case.title}}\"},\"footer\":{\"template\":\"Confidential - {{case.title}}\"},\"dateFormat\":\"yyyy-MM-dd\",\"dateTimeFormat\":\"yyyy-MM-dd HH:mm\",\"i18n\":{\"lang\":\"en\"}},\"version\":1}");
		AssertFullTemplate(result);
	}

	[Fact]
	public async Task CreateAsync_RequiredOnly_OmitsOptionals_And_AbsentOptionalsMapToDefaults()
	{
		var stub = Stub(HttpStatusCode.Created, MinimalTemplateJson);
		using var client = TestClient.Create(stub);

		var result = await client.CaseReportTemplates.CreateAsync(
			new CaseReportTemplateCreateRequest { Title = "t", Definition = new CaseReportTemplateDefinition() },
			TestContext.Current.CancellationToken);

		stub.Calls[0].Body.Should().Be("""{"title":"t","definition":{"i18n":{}}}""");
		result.UpdatedBy.Should().BeNull();
		result.UpdatedAt.Should().BeNull();
		result.Definition.Widgets.Should().BeNull();
		result.Definition.Header.Should().BeNull();
		result.Definition.Footer.Should().BeNull();
		result.Definition.DateFormat.Should().BeNull();
		result.Definition.DateTimeFormat.Should().BeNull();
		result.Definition.I18n.Lang.Should().BeNull();
	}

	[Fact]
	public void Models_Defaults_AreEmptyNotNull()
	{
		var item = new CaseReportTemplate();
		item.Id.Should().BeEmpty();
		item.Type.Should().BeEmpty();
		item.CreatedBy.Should().BeEmpty();
		item.Title.Should().BeEmpty();
		item.Group.Should().BeEmpty();
		item.Description.Should().BeEmpty();
		item.Definition.Should().NotBeNull();
		item.Definition.I18n.Should().NotBeNull();
		new CaseReportHeader().Template.Should().BeEmpty();
		new CaseReportFooter().Template.Should().BeEmpty();

		var options = new CaseReportTemplateOptions();
		options.Widgets.Should().BeEmpty();
		options.AlertFields.Should().BeEmpty();
		options.ObservableFields.Should().BeEmpty();
		options.TaskFields.Should().BeEmpty();
		options.TtpFields.Should().BeEmpty();
		options.LogFields.Should().BeEmpty();
		options.TimelineEvents.Should().BeEmpty();
	}

	[Fact]
	public void WidgetKinds_AreTheSpecValues()
	{
		string[] kinds =
		[
			CaseReportWidgetKinds.Text, CaseReportWidgetKinds.Image, CaseReportWidgetKinds.CustomFields, CaseReportWidgetKinds.CustomFieldsList,
			CaseReportWidgetKinds.AlertTable, CaseReportWidgetKinds.AlertList, CaseReportWidgetKinds.ObservableTable, CaseReportWidgetKinds.ObservableList,
			CaseReportWidgetKinds.TaskTable, CaseReportWidgetKinds.TaskList, CaseReportWidgetKinds.TTPTable, CaseReportWidgetKinds.TTPList,
			CaseReportWidgetKinds.Timeline, CaseReportWidgetKinds.Comments, CaseReportWidgetKinds.Pages
		];

		kinds.Should().Equal(
			"Text", "Image", "CustomFields", "CustomFieldsList", "AlertTable", "AlertList", "ObservableTable", "ObservableList",
			"TaskTable", "TaskList", "TTPTable", "TTPList", "Timeline", "Comments", "Pages");
	}

	[Fact]
	public async Task GetAsync_FullTemplate_MapsEveryField_And_UnmodelledWidgetMembersRoundTrip()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, TemplateJson);
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		var result = await client.CaseReportTemplates.GetAsync("~42123", TestContext.Current.CancellationToken);
		await client.CaseReportTemplates.UpdateAsync("~42123", new CaseReportTemplateUpdateRequest { Definition = result.Definition }, TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123");
		stub.Calls[0].Body.Should().BeNull();
		AssertFullTemplate(result);
		stub.Calls[1].Body.Should().Contain("""{"_kind":"FutureWidget","novelty":{"level":3}}""");
	}

	[Fact]
	public async Task GetOptionsAsync_MapsEveryList()
	{
		var stub = Stub(HttpStatusCode.OK, """
			{
				"widgets":["Text","Image"],"alertFields":["_id","title"],"observableFields":["dataType","data"],
				"taskFields":["title","status"],"ttpFields":["tactic","patternId"],"logFields":["message","date"],
				"timelineEvents":["Alert","Case"]
			}
			""");
		using var client = TestClient.Create(stub);

		var result = await client.CaseReportTemplates.GetOptionsAsync(TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/_info");
		stub.Calls[0].Body.Should().BeNull();
		result.Widgets.Should().Equal("Text", "Image");
		result.AlertFields.Should().Equal("_id", "title");
		result.ObservableFields.Should().Equal("dataType", "data");
		result.TaskFields.Should().Equal("title", "status");
		result.TtpFields.Should().Equal("tactic", "patternId");
		result.LogFields.Should().Equal("message", "date");
		result.TimelineEvents.Should().Equal("Alert", "Case");
	}

	[Fact]
	public async Task UpdateAsync_PatchesEveryProperty()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReportTemplates.UpdateAsync(
			"~42123",
			new CaseReportTemplateUpdateRequest
			{
				Title = "New title",
				Group = "New group",
				Description = "New description",
				Definition = new CaseReportTemplateDefinition { Footer = new CaseReportFooter { Template = "f" } },
				Version = 2
			},
			TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123");
		stub.Calls[0].Body.Should().Be(
			"""{"title":"New title","group":"New group","description":"New description","definition":{"footer":{"template":"f"},"i18n":{}},"version":2}""");
	}

	[Fact]
	public async Task UpdateAsync_Empty_SendsEmptyObject()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReportTemplates.UpdateAsync("Ransomware Report", new CaseReportTemplateUpdateRequest(), TestContext.Current.CancellationToken);

		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/Ransomware%20Report");
		stub.Calls[0].Body.Should().Be("{}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReportTemplates.DeleteAsync("~42123", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task AddAttachmentsAsync_UploadsEachFileAsAnAttachmentsPart()
	{
		var stub = Stub(HttpStatusCode.Created, $$"""{"attachments":[{{AttachmentJson}}]}""");
		using var client = TestClient.Create(stub);
		using var first = new MemoryStream([1, 2, 3]);
		using var second = new MemoryStream([4, 5]);

		var result = await client.CaseReportTemplates.AddAttachmentsAsync(
			"~42123",
			[new StreamPart(first, "logo.png", "image/png"), new StreamPart(second, "banner.jpg", "image/jpeg")],
			canRename: true,
			TestContext.Current.CancellationToken);

		var call = stub.Calls[0];
		call.Method.Should().Be(HttpMethod.Post);
		call.Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123/attachment");
		call.ContentType.Should().Be("multipart/form-data");
		call.Parts.Should().HaveCount(3);
		call.Parts[0].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "logo.png" && p.ContentType == "image/png");
		call.Parts[0].Bytes.Should().Equal(1, 2, 3);
		call.Parts[1].Should().Match<RecordedPart>(p => p.Name == "attachments" && p.FileName == "banner.jpg" && p.ContentType == "image/jpeg");
		call.Parts[1].Bytes.Should().Equal(4, 5);
		call.Parts[2].Name.Should().Be("canRename");
		call.Parts[2].FileName.Should().BeNull();
		call.Parts[2].Text.Should().Be("true");
		var attachment = result.Attachments.Should().ContainSingle().Subject;
		attachment.Id.Should().Be("~456789012");
		attachment.Name.Should().Be("logo.png");
		attachment.Size.Should().Be(2048);
	}

	[Fact]
	public async Task AddAttachmentsAsync_WithoutCanRename_SendsOnlyFiles()
	{
		var stub = Stub(HttpStatusCode.Created, """{"attachments":[]}""");
		using var client = TestClient.Create(stub);

		await client.CaseReportTemplates.AddAttachmentsAsync(
			"~42123",
			[new ByteArrayPart([9], "a.bin")],
			cancellationToken: TestContext.Current.CancellationToken);

		var part = stub.Calls[0].Parts.Should().ContainSingle().Subject;
		part.Name.Should().Be("attachments");
		part.FileName.Should().Be("a.bin");
		part.Bytes.Should().Equal(9);
	}

	[Fact]
	public async Task DeleteAttachmentAsync_SendsDelete()
	{
		var stub = Stub(HttpStatusCode.NoContent);
		using var client = TestClient.Create(stub);

		await client.CaseReportTemplates.DeleteAttachmentAsync("~42123", "~456789012", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123/attachment/~456789012");
		stub.Calls[0].Body.Should().BeNull();
	}

	[Fact]
	public async Task GetAttachmentAsync_ReturnsExactBytes_WithoutConditionalHeaderByDefault()
	{
		var stub = new StubHandler();
		byte[] image = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];
		stub.EnqueueFile(image, "image/png", "logo.png");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReportTemplates.GetAttachmentAsync("~42123", "~456789012", cancellationToken: TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123/attachment/~456789012");
		stub.Calls[0].Uri.Query.Should().BeEmpty();
		stub.Calls[0].Headers.Contains("If-None-Match").Should().BeFalse();
		content.Headers.ContentType!.MediaType.Should().Be("image/png");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(image);
	}

	[Fact]
	public async Task GetAttachmentAsync_NotModified_SendsIfNoneMatchAndThrows()
	{
		var stub = Stub(HttpStatusCode.NotModified);
		using var client = TestClient.Create(stub);

		var act = () => client.CaseReportTemplates.GetAttachmentAsync("~42123", "~456789012", "\"abc123\"", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.StatusCode.Should().Be(HttpStatusCode.NotModified);
		stub.Calls[0].Headers.GetValues("If-None-Match").Should().Equal("\"abc123\"");
	}

	[Fact]
	public async Task DownloadAttachmentAsync_ReturnsExactBytesAndFileName()
	{
		var stub = new StubHandler();
		byte[] image = [0x89, 0x50, 0x4E, 0x47, 0x00, 0xFF];
		stub.EnqueueFile(image, "application/octet-stream", "logo.png");
		using var client = TestClient.Create(stub);

		using var content = await client.CaseReportTemplates.DownloadAttachmentAsync("~42123", "~456789012", TestContext.Current.CancellationToken);

		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/caseReportTemplate/~42123/attachment/~456789012/download");
		content.Headers.ContentType!.MediaType.Should().Be("application/octet-stream");
		content.Headers.ContentDisposition!.FileName.Should().Be("logo.png");
		(await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(image);
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = Stub(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"CaseReportTemplate ~1 not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.CaseReportTemplates.GetAsync("~1", TestContext.Current.CancellationToken);

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
