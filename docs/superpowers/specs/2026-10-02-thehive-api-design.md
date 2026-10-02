# TheHive.Api: design

Date: 2026-10-02. Ticket: OPS-157846 (dev/test environment CR, in progress).

## Goal
A .NET 10 NuGet package, `TheHive.Api`, providing a typed, hand-written Refit client for the full
TheHive 5 REST API (StrangeBee). Public repo `panoramicdata/TheHive.Api`, MIT, cloned at
`..\TheHive.Api`. It follows `NEW_NUGETS.md` and Highlight.Api conventions.

## Success criteria
1. 0 Codacy issues (Codacy added to the repo; `.codacy.yaml` copied from Highlight.Api).
2. 100% line and branch coverage from unit tests alone, with no coverage exclusions of product code.
3. `dotnet build -c Release` with `TreatWarningsAsErrors`: 0 warnings. `dotnet pack` yields .nupkg and .snupkg.
4. Public API fully XML-documented.
5. The repo passes NugetManagement governance scanning.

## Scope
Hand-written coverage of the whole TheHive 5 API. The endpoint checklist is the official OpenAPI spec
for 5.6.x (as used by thehive4go v0.56), cross-checked against thehive4py's 14 endpoint groups.
Groups: Alerts, Cases, CaseTemplates, Tasks, TaskLogs, Observables, Comments, Attachments,
CustomFields, Users, Organisations, Profiles, Procedures, Patterns, Timeline, Dashboards, Cortex,
Query/Export, Admin. The group list is finalised against the spec at build time and recorded in
`docs/endpoint-coverage.md` (spec operation -> client method), so completeness is auditable.

Out of scope: TheHive 4 API; code generation; webhooks/notification receivers.

## Structure
- `TheHive.Api.slnx`
- `TheHive.Api/` (package): `Interfaces/` (Refit, one per group), `Data/` (models and requests),
  `Query/` (fluent `QueryBuilder`), `Converters/`, `Handlers/`, `TheHiveClient`, `TheHiveClientOptions`,
  `TheHiveApiException`.
- `TheHive.Api.Test/`: unit tests, `failSkips:true`.
- `TheHive.Api.Test.Integration/`: separate project, `failSkips:false`, user-secrets config,
  inert until the OPS-157846 environment exists. Does not count toward coverage.

## Components
- **TheHiveClient(options)** exposes one property per group.
- **TheHiveClientOptions**: BaseUrl, ApiKey, Organisation (`X-Organisation`), Timeout, MaxRetries,
  retry back-off, optional `ILogger`. Validated on construction.
- **AuthRetryHandler (DelegatingHandler)**: bearer key, org header, retry on 429/5xx with
  exponential back-off honouring `Retry-After`, masked logging (never logs the key).
- **Serialisation**: System.Text.Json; explicit mapping of `_id`, `_type`, `_createdAt` etc.;
  converters for epoch-millisecond timestamps and tolerant enums (unknown value -> `Unknown`, not a throw).
- **TheHiveApiException**: status code, TheHive `type` and `message`, request id.
- **QueryBuilder**: typed builder over `/api/v1/query` steps (list, filter, sort, page, select, count, ...).

## Data flow
Caller -> `client.Cases.GetAsync(id)` -> Refit -> AuthRetryHandler -> HttpClient -> TheHive.
Non-success responses become `TheHiveApiException`. JSON is mapped to `Data/` models.

## Testing
- TDD, strict AwesomeAssertions. Each endpoint has a test using a fake `HttpMessageHandler` that
  asserts method, path, query, headers and body, and that the canned response maps to the model.
- Handler tests cover retry, back-off, `Retry-After`, exhaustion, key masking, and cancellation.
- Converter and options tests cover every branch, including malformed input.
- Coverage: `dotnet test --coverage --coverage-output-format cobertura`, with a CI gate that fails below 100%.
- A reflection test asserts that every Refit interface method is exercised and documented.

## Repo and CI
Copied from Highlight.Api and `NEW_NUGETS.md`: `.editorconfig`, `global.json`, NBGV `version.json`,
`Directory.Build.props`, CPM `Directory.Packages.props`, README (badges, quick start), CONTRIBUTING,
SECURITY, LICENSE, `Publish.ps1`, `ci.yml` (restore, build, test with coverage gate, pack, tag-triggered
trusted publish), `codeql.yml`, `dependabot.yml`, `.github/copilot-instructions.md`, Codacy config.
Created with `gh repo create panoramicdata/TheHive.Api --public --clone`.
Nothing is published to nuget.org until the user says so.

## Build order
1. Repo, governance files, empty solution with a green CI.
2. Core: options, handler, converters, exception, client shell, and their tests.
3. Endpoint groups one at a time (models, interface, tests), core IR groups first: Cases, Alerts,
   Tasks, TaskLogs, Observables, Comments, Attachments.
4. Query builder.
5. Remaining groups.
6. Coverage and Codacy sweep to 100% and 0 issues; README; endpoint-coverage doc.
7. Integration tests, once the OPS-157846 environment is ready.

## Risks and assumptions
- The OpenAPI spec is the source of truth. If it can't be fetched (it is served from a TheHive instance),
  I'll derive it from thehive4go's repo scripts or ask for access once the test environment is up.
- Some response shapes are only verifiable against a live instance. Those are marked and re-checked by
  the integration tests when the environment exists.
- "Full API" is large, so work proceeds in reviewable commits per group.
