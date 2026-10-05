# TheHive.Api

<!-- TODO(owner): add Codacy grade/coverage badges once the repo has a Codacy project id -->
[![NuGet](https://img.shields.io/nuget/v/TheHive.Api.svg)](https://www.nuget.org/packages/TheHive.Api)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)

A strongly typed .NET 10 client for the [TheHive 5](https://strangebee.com/thehive/) REST API (StrangeBee), published by Panoramic Data Limited.

It covers every non-deprecated operation of the TheHive 5.8.0 OpenAPI specification (264 operations across 41 groups), exposes each group as a [Refit](https://github.com/reactiveui/refit)-backed interface, uses `System.Text.Json`, and handles bearer authentication, the organisation header and transient-failure retries for you.

> **Status:** nothing has been published to NuGet yet. Until the first release, build from source.

## Installation

```sh
dotnet add package TheHive.Api
```

Requires .NET 10.

## Targeted TheHive version

The client is generated from the TheHive **v5.8.0** OpenAPI specification (`docs/openapi/thehive-docs.yaml`, OpenAPI 3.1.0) and has been verified against a live TheHive **5.8.0** instance. Operations that are newer than the server you run, or that need a licence tier your server lacks, are answered by the server with an error (raised as `TheHiveApiException`).

## Versioning

The package's major.minor follows the TheHive API version the client targets: `5.8` means the v5.8.0 specification. The patch number is the Git height assigned by [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) (`5.8.<height>`). Breaking changes to this client made before it targets the next TheHive release therefore ship as patch bumps: the package version does not follow SemVer for the client's own API, so read the release notes before upgrading.

### Breaking changes

- **Versions after 5.8.9** (5.8.9 is the last release published before this change): every method takes a required `CancellationToken` as its last parameter (pass `CancellationToken.None` if you do not need cancellation), and operations with optional query, header or multipart parameters take an options object instead (for example `IStatus.GetAsync(new PlatformStatusQuery { Verbose = true }, cancellationToken)`, `IUsers.GetAvatarAsync(userId, file, new ConditionalDownloadOptions { IfNoneMatch = etag }, cancellationToken)` or `ICases.AddAttachmentsAsync(caseId, files, new AttachmentUploadOptions { CanRename = true }, cancellationToken)`); pass `new()` when you have no options. The query helpers take a `QueryRunOptions` for the optional query `name`, and `QueryBuilder.Sort(field)` / `Sort(field, direction)` are overloads. The requests on the wire are unchanged.

## Quick start

Create a client. `BaseUrl` and `ApiKey` are required; `Organisation` is optional and sent as the `X-Organisation` header. Every method takes a required `CancellationToken`; the examples use `CancellationToken.None`, but pass your host's token (for example `HttpContext.RequestAborted` or a `CancellationTokenSource`) in real code.

```csharp
using System.Net;
using TheHive.Api;
using TheHive.Api.Data.Cases;
using TheHive.Api.Data.Common;
using TheHive.Api.Data.Query;
using TheHive.Api.Querying;

using var client = new TheHiveClient(new TheHiveClientOptions
{
	BaseUrl = Environment.GetEnvironmentVariable("THEHIVE_BASE_URL")!,
	ApiKey = Environment.GetEnvironmentVariable("THEHIVE_API_KEY")!,
	Organisation = Environment.GetEnvironmentVariable("THEHIVE_ORGANISATION")
});
var cancellationToken = CancellationToken.None;
```

Create a case:

```csharp
var created = await client.Cases.CreateAsync(new CaseCreateRequest
{
	Title = "Suspicious login from a new country",
	Description = "Raised from the SIEM; needs triage.",
	Severity = Severity.High,
	Tlp = Tlp.Amber,
	Tags = ["siem", "login"]
}, cancellationToken);
Console.WriteLine($"Created case {created.Id} (#{created.Number})");
```

Query cases with the fluent `QueryBuilder` (`POST /api/v1/query`) and read the result as typed cases:

```csharp
var query = QueryBuilder.ListCases()
	.FilterLike("title", "login")
	.Sort("_createdAt", SortDirection.Descending)
	.Page(0, 15);
var cases = await client.Query.RunAsync<Case>(query, new QueryRunOptions(), cancellationToken);
foreach (var item in cases)
{
	Console.WriteLine($"{item.Id}: {item.Title}");
}
```

Update a case. Update models use `Optional<T>` for fields the API lets you clear: leave the property unset to keep the field, set a value to change it, or set `null` explicitly to clear it.

```csharp
await client.Cases.UpdateAsync(created.Id, new CaseUpdateRequest
{
	Title = "Suspicious login (confirmed)",       // plain property: sent only when not null
	Summary = new Optional<string?>("Triaged."),  // set a value
	Assignee = new Optional<string?>(null)        // clear the assignee (sends "assignee": null)
}, cancellationToken);
```

Download a case export (a THAR archive; a Gold or Platinum licence is needed). The caller owns and disposes the returned content:

```csharp
using var export = await client.Cases.ExportAsync(created.Id, "choose-a-password", cancellationToken);
await using var file = File.Create("case-export.thar");
await using var source = await export.ReadAsStreamAsync(cancellationToken);
await source.CopyToAsync(file, cancellationToken);
Console.WriteLine($"Suggested name: {export.Headers.ContentDisposition?.FileName}");
```

Every non-success response is raised as `TheHiveApiException`, carrying the status code, TheHive's error `type` and the `X-Request-Id`:

```csharp
try
{
	await client.Cases.GetAsync("~does-not-exist", cancellationToken);
}
catch (TheHiveApiException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
	Console.WriteLine($"Not found ({ex.ErrorType}), request {ex.RequestId}: {ex.Message}");
}
catch (TheHiveApiException ex)
{
	Console.WriteLine($"{(int)ex.StatusCode} {ex.ErrorType}: {ex.Message}");
}
```

## Capabilities

Every operation of the spec is accounted for: 272 operations = 264 implemented + 8 deprecated and skipped on purpose. Each group is a property of `TheHiveClient`. The per-operation table (method and test for each endpoint) is in [docs/endpoint-coverage.md](docs/endpoint-coverage.md), which is the source of truth and is enforced by `InventoryTests`.

| Spec tag | Interface | Property | Implemented | Deprecated (skipped) |
|---|---|---|---|---|
| Admin | `IAdmin` | `client.Admin` | 1 | 0 |
| Alert | `IAlerts` | `client.Alerts` | 15 | 2 |
| Alert Feeder | `IAlertFeeders` | `client.AlertFeeders` | 6 | 0 |
| AlertStatus | `IAlertStatuses` | `client.AlertStatuses` | 3 | 0 |
| Att&ck | `IPatterns` | `client.Patterns` | 7 | 0 |
| Audit | `IAudit` | `client.Audit` | 1 | 0 |
| Authentication | `IAuthentication` | `client.Authentication` | 8 | 0 |
| Branding | `IBranding` | `client.Branding` | 4 | 0 |
| Case | `ICases` | `client.Cases` | 25 | 3 |
| Case Report | `ICaseReports` | `client.CaseReports` | 8 | 0 |
| Case Report Template | `ICaseReportTemplates` | `client.CaseReportTemplates` | 9 | 0 |
| CaseStatus | `ICaseStatuses` | `client.CaseStatuses` | 3 | 0 |
| CaseTemplate | `ICaseTemplates` | `client.CaseTemplates` | 5 | 0 |
| Comment | `IComments` | `client.Comments` | 4 | 0 |
| Config | `IConfig` | `client.Config` | 3 | 0 |
| Cortex | `ICortex` | `client.Cortex` | 16 | 0 |
| CustomField | `ICustomFields` | `client.CustomFields` | 4 | 0 |
| Dashboard | `IDashboards` | `client.Dashboards` | 5 | 0 |
| Describe | `IDescribe` | `client.Describe` | 2 | 0 |
| Email Intake | `IEmailIntake` | `client.EmailIntake` | 11 | 0 |
| Function | `IFunctions` | `client.Functions` | 8 | 0 |
| License | `ILicense` | `client.License` | 6 | 0 |
| MISP | `IMisp` | `client.Misp` | 4 | 0 |
| Observable | `IObservables` | `client.Observables` | 7 | 0 |
| Observable Type | `IObservableTypes` | `client.ObservableTypes` | 4 | 0 |
| Organization | `IOrganisations` | `client.Organisations` | 13 | 0 |
| Page | `IPages` | `client.Pages` | 6 | 0 |
| PageTemplate | `IPageTemplates` | `client.PageTemplates` | 3 | 0 |
| Permission | `IPermissions` | `client.Permissions` | 1 | 0 |
| Profile | `IProfiles` | `client.Profiles` | 4 | 0 |
| Query and Export | `IQuery` | `client.Query` | 3 | 0 |
| Share | `IShares` | `client.Shares` | 13 | 0 |
| Status | `IStatus` | `client.Status` | 2 | 0 |
| Tag | `ITags` | `client.Tags` | 3 | 0 |
| Task | `ITasks` | `client.Tasks` | 8 | 0 |
| Task Log | `ITaskLogs` | `client.TaskLogs` | 6 | 2 |
| Taxonomy | `ITaxonomies` | `client.Taxonomies` | 6 | 0 |
| Timeline | `ITimeline` | `client.Timeline` | 3 | 0 |
| TTP | `IProcedures` | `client.Procedures` | 7 | 0 |
| User | `IUsers` | `client.Users` | 13 | 1 |
| Views | `IViews` | `client.Views` | 4 | 0 |
| **Total** | | | **264** | **8** |

Notes: operations tagged `Share` in the spec (cases, tasks, observables) are on `IShares`; the spec's `TTP` tag is `IProcedures` and `Att&ck` is `IPatterns`. Querying is done with `QueryBuilder` plus the `QueryExtensions` on `client.Query` (`RunAsync<T>`, `RunCountAsync`, ...).

## Retries, timeouts, logging

All of this is implemented in one `DelegatingHandler` and configured through `TheHiveClientOptions`:

- **Retries** (`MaxRetries`, default 3; `RetryBaseDelay`, default 1 s, must not be negative, zero means no waiting; doubled on each retry; `Retry-After` is honoured when present). Every wait, back-off or `Retry-After`, is capped at `MaxRetryDelay` (default 30 s, must be greater than zero), so growth cannot overflow and a huge `Retry-After` cannot stall a caller. The options are read once when the client is constructed; changing the options object later has no effect.
  - 429 and 503 are retried for every verb, including `POST` and `PATCH`, because the server did not process the request.
  - Other 5xx responses are retried only for idempotent verbs (`GET`, `HEAD`, `PUT`, `DELETE`, `OPTIONS`, `TRACE`), never `POST` or `PATCH`.
  - A request whose body cannot be replayed (a stream or multipart upload, a case import) is **never retried**, so an upload is never sent twice; its first error response is raised as `TheHiveApiException`.
- **Timeout** (`Timeout`, default 100 s) applies **per attempt** and covers sending the request and receiving the response headers only. It excludes back-off and `Retry-After` waits and the time spent reading a download body; pass a `CancellationToken` to `ReadAs*Async` to bound that. A timed-out attempt raises `TimeoutException`; caller cancellation still raises `OperationCanceledException`. Large uploads need a larger `Timeout`.
- **Logging** (`Logger`, an optional `Microsoft.Extensions.Logging.ILogger`): the verb, request **path** and attempt number are logged at Debug, and retries at Warning. The query string (which can hold secrets such as an export password) and the API key are never logged.

## Security notes

- The API key is sent as a bearer token (`Authorization: Bearer ...`) and is never written to logs or to `TheHiveClientOptions.ToString()`.
- Request query strings are never logged (see above).
- Models carry secrets only where the spec returns them (for example connector credentials); each such property is documented `SECRET` in its XML comment. Do not log or persist those models.
- The organisation is sent as the `X-Organisation` header on every request when `Organisation` is set.
- Please report vulnerabilities privately; see [SECURITY.md](SECURITY.md).

## Known limitations

- The object form of `customFields` (`{"name": value}`), which the spec allows as an alternative to the array form in several inputs, is not modelled; only the array form (`List<CustomFieldInput>`) is sent.
- Where the spec offers separate multipart-only and JSON-only variants of a create operation, each variant is exposed by its own method, so choose the one that fits your payload.
- Operations marked `(verify)` in [docs/endpoint-coverage.md](docs/endpoint-coverage.md) rest on untyped or example-derived spec content and have not all been confirmed against a live server.
- Enumerations are tolerant: a value this client does not know reads as `Unknown`. Writing a read model back sends `Unknown`, so do not round-trip read models into create or update requests blindly.
- The 8 deprecated operations are not implemented.
- The `_type` property is named differently by type: when a schema also has its own `type` property, `_type` maps to `EntityType` and `type` to `Type` (for example `Alert.EntityType`); otherwise `_type` maps to `Type` (for example `Case.Type`). The rule is in the Implementation notes of [docs/endpoint-coverage.md](docs/endpoint-coverage.md).
- Some models have `required` members (for example `Access`, `OrganisationLink` and several report, feeder and mailbox nested types). They rely on `TheHiveJson.Options` ignoring `required` when reading, so anyone deserialising these types with their own `JsonSerializerOptions` must use `TheHiveJson.Options` (it is read-only; copy it with `new JsonSerializerOptions(TheHiveJson.Options)` to customise).
- `POST` and `PATCH` are retried only on 429 and 503. If a proxy answers 503 after the server already committed a create, the retry can create a duplicate. Set `MaxRetries = 0` to disable retries.

## Testing and quality

- Unit tests (`TheHive.Api.Test`, xunit v3 with AwesomeAssertions) stub the HTTP layer; there are no skipped tests and a gate in CI enforces **100% line and branch coverage** (`scripts/Check-Coverage.ps1`).
- The build uses `TreatWarningsAsErrors` and must produce zero diagnostics.
- A separate project, `TheHive.Api.Test.Integration`, runs read and write checks against a real TheHive 5 instance. It is not part of CI and **skips** every test when no secrets are configured. To run it, store your own values in user-secrets (placeholders shown):

```sh
dotnet user-secrets set "Config:BaseAddress" "<url>" --project TheHive.Api.Test.Integration
dotnet user-secrets set "Config:ApiKey" "<api key>" --project TheHive.Api.Test.Integration
dotnet user-secrets set "Config:Organisation" "<organisation name>" --project TheHive.Api.Test.Integration
dotnet test --project TheHive.Api.Test.Integration
```

It creates only items named `[TheHive.Api integration] <guid>` and deletes them afterwards. Never commit real values.

## Links

- NuGet: https://www.nuget.org/packages/TheHive.Api
- Source: https://github.com/panoramicdata/TheHive.Api
- Issues: https://github.com/panoramicdata/TheHive.Api/issues
- Contributing: [CONTRIBUTING.md](CONTRIBUTING.md); publishing: [PUBLISHING.md](PUBLISHING.md)

## License

Released under the [MIT License](LICENSE). Copyright (c) 2026 Panoramic Data Limited.
