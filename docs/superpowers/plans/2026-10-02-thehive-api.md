# TheHive.Api Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship `TheHive.Api`, a hand-written Refit client for the full TheHive 5 REST API, with 0 Codacy issues and 100% line and branch coverage.

**Architecture:** One Refit interface per resource group under `Interfaces/`, models under `Data/`, exposed as properties on `TheHiveClient`. A `DelegatingHandler` does auth, organisation header, retry and masked logging. Refit's `ExceptionFactory` maps failures to `TheHiveApiException`. A fluent `QueryBuilder` drives `/api/v1/query`. Unit tests use a stub `HttpMessageHandler`, so no live TheHive is needed.

**Tech Stack:** .NET 10, Refit 15.2.0, System.Text.Json, xunit.v3 4.0.0 on Microsoft.Testing.Platform, AwesomeAssertions 9.6.0, Microsoft.Testing.Extensions.CodeCoverage 18.10.0, Nerdbank.GitVersioning 3.10.91, central package management.

**Spec:** `docs/superpowers/specs/2026-10-02-thehive-api-design.md`

## Global Constraints

- `TargetFramework` is `net10.0`; `TreatWarningsAsErrors` true; `Nullable` enable; `GenerateDocumentationFile` true. **Do not** add `NoWarn CS1591`: every public member needs XML docs.
- Tabs for indentation in C#, XML, JSON and MSBuild (see `.editorconfig`); spaces only in YAML. File-scoped namespaces. Do not reformat unrelated code.
- No `Version=` in any `PackageReference`; all versions live in `Directory.Packages.props`, one per line.
- No coverage exclusions of product code, and no Codacy `exclude_paths` for `Interfaces/` or `Data/` (Highlight.Api does this; do **not** copy that).
- Unit test project: `xunit.runner.json` has `"failSkips": true`. Never skip or delete a test to pass a build.
- Test project uses `AwesomeAssertions` (not FluentAssertions); no `xunit.runner.visualstudio`, no coverlet.
- `global.json` must contain `"test": { "runner": "Microsoft.Testing.Platform" }`.
- Build only the affected project when verifying (repo instruction), e.g. `dotnet build TheHive.Api/TheHive.Api.csproj`; run tests with `dotnet test --project TheHive.Api.Test`. Read the exit code (5 = MTP config problem, 2 = real failures).
- Never commit secrets; the integration project reads user-secrets only.
- Commits end with: `Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>`
- Work on `main` only if the user says so; otherwise use a feature branch `feat/initial-client` and open a PR. Do not push tags or publish to nuget.org.

## Review Focus

1. TheHive timestamps arrive as epoch **milliseconds**, and some are null or absent: converter must round-trip and tolerate null.
2. Unknown enum strings from a newer TheHive (e.g. a new case status) must not throw: they map to `Unknown`.
3. `429` with `Retry-After` given as seconds **or** an HTTP date, and `5xx` after retries are exhausted: wait honestly, then throw `TheHiveApiException`, never loop forever.
4. A non-JSON error body (HTML from a proxy, empty body): `TheHiveApiException` still raised with status code and a fallback message.
5. The API key must never appear in logs or exception messages, including via `ToString()` on options.
6. A `BaseUrl` without a trailing slash, or with a path prefix (`https://host/thehive`): relative Refit paths must still resolve.
7. Cancellation during a retry back-off must cancel promptly, not wait out the delay.

---

## File Structure

```
.editorconfig  .gitattributes  .gitignore  .codacy.yaml  global.json  version.json
Directory.Build.props  Directory.Packages.props  TheHive.Api.slnx
LICENSE  README.md  CONTRIBUTING.md  SECURITY.md  Publish.ps1
.github/{workflows/ci.yml, workflows/codeql.yml, dependabot.yml, copilot-instructions.md}
scripts/Check-Coverage.ps1  coverage.settings.xml
docs/endpoint-coverage.md
TheHive.Api/
  TheHive.Api.csproj  Logo.png
  TheHiveClient.cs  TheHiveClientOptions.cs  TheHiveApiException.cs  TheHiveErrorMapper.cs  TheHiveJson.cs
  Handlers/AuthRetryHandler.cs
  Converters/EpochMillisecondsConverter.cs  Converters/TolerantEnumConverterFactory.cs
  Query/QueryBuilder.cs  Query/QueryStep.cs  Query/QuerySteps.cs
  Interfaces/I{Group}.cs      (one per group)
  Data/{Group}/*.cs           (one folder per group)
TheHive.Api.Test/
  TheHive.Api.Test.csproj  xunit.runner.json  Support/StubHandler.cs  Support/TestClient.cs
  Core/*Tests.cs  Query/*Tests.cs  Groups/{Group}Tests.cs
TheHive.Api.Test.Integration/
  TheHive.Api.Test.Integration.csproj  xunit.runner.json (failSkips false)  usersecrets.example.json  *.cs
```

## Group Recipe (used by Tasks 7-9)

Every endpoint group follows exactly the pattern worked in Task 6 (Cases). For each group:

1. Look the group's operations up in the OpenAPI spec (Task 3 output, `docs/endpoint-coverage.md`). The endpoint tables in Tasks 7-9 are the **minimum**; add every extra operation the spec lists for that group, as rows in `docs/endpoint-coverage.md`.
2. Write models in `Data/{Group}/` (sealed classes, `init` or `set` properties, `[JsonPropertyName]` for each wire name, XML docs on every public member, nullable where the spec says optional).
3. For **each** operation write one test in `TheHive.Api.Test/Groups/{Group}Tests.cs` using `TestClient.Create(stub)`: enqueue a canned JSON response, call the method, assert `stub.Calls[0]` method/path/query/body and every mapped property of the result. Add one failure test per group (e.g. 404 -> `TheHiveApiException`).
4. Run the group's tests, watch them fail, write the Refit interface `I{Group}` (XML-documented, one method per operation, `CancellationToken` last parameter), add the property to `TheHiveClient` and `RestService.For<I{Group}>` in its constructor.
5. Run the tests green, then `dotnet test --project TheHive.Api.Test --coverage ...` and confirm the new files are 100%.
6. Update `docs/endpoint-coverage.md` rows (`spec operation | client method | test`) and commit: `feat: add {Group} endpoints`.

---

### Task 1: Repository skeleton and governance files

**Files:**
- Create: everything under "File Structure" at repo root except `TheHive.Api/` and test projects' source files (solution, props, global.json, version.json, license, docs, workflows)
- Copy from `..\Highlight.Api\`: `.editorconfig`, `.gitattributes`, `.gitignore`, `LICENSE`, `.github/dependabot.yml`, `.github/workflows/codeql.yml`, `TheHive.Api/Logo.png` (replace Logo later if a branded one exists)

**Interfaces:**
- Produces: a solution that builds with `TheHive.Api` (empty) and `TheHive.Api.Test` (one trivial passing test), `Directory.Packages.props` with all versions used later.

- [ ] **Step 1: Create branch and copy boilerplate**

```bash
cd /c/Users/david/source/repos/panoramicdata/TheHive.Api
git checkout -b feat/initial-client
H=../Highlight.Api
cp $H/.editorconfig $H/.gitattributes $H/.gitignore $H/LICENSE $H/global.json .
mkdir -p .github/workflows TheHive.Api TheHive.Api.Test scripts
cp $H/.github/dependabot.yml .github/
cp $H/.github/workflows/codeql.yml .github/workflows/
cp $H/Highlight.Api/Logo.png TheHive.Api/Logo.png
```
Then in `LICENSE` change nothing except confirm it names Panoramic Data Limited. In `.github/workflows/codeql.yml` and `dependabot.yml` replace `Highlight` with `TheHive` (`grep -n Highlight` must return nothing).

- [ ] **Step 2: Write `version.json`**

```json
{
	"version": "1.0",
	"publicReleaseRefSpec": [
		"^refs/heads/main$",
		"^refs/tags/\\d+\\.\\d+\\.\\d+$"
	]
}
```

- [ ] **Step 3: Write `Directory.Build.props`** (note: no `NoWarn`)

```xml
<Project>

	<PropertyGroup>
		<Authors>Panoramic Data Limited</Authors>
		<Company>Panoramic Data Limited</Company>
		<Copyright>Copyright © $([System.DateTime]::Now.Year) Panoramic Data Limited</Copyright>
		<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
		<Nullable>enable</Nullable>
		<NuGetAuditMode>All</NuGetAuditMode>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
	</PropertyGroup>

</Project>
```

- [ ] **Step 4: Write `Directory.Packages.props`**

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project>
	<PropertyGroup>
		<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
	</PropertyGroup>
	<ItemGroup>
		<PackageVersion Include="AwesomeAssertions" Version="9.6.0" />
		<PackageVersion Include="AwesomeAssertions.Analyzers" Version="9.0.8" />
		<PackageVersion Include="Microsoft.Extensions.Configuration" Version="10.0.11" />
		<PackageVersion Include="Microsoft.Extensions.Configuration.UserSecrets" Version="10.0.11" />
		<PackageVersion Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.11" />
		<PackageVersion Include="Microsoft.NET.Test.Sdk" Version="18.9.0" />
		<PackageVersion Include="Microsoft.Testing.Extensions.CodeCoverage" Version="18.10.0" />
		<PackageVersion Include="Nerdbank.GitVersioning" Version="3.10.91" />
		<PackageVersion Include="Refit" Version="15.2.0" />
		<PackageVersion Include="xunit.v3" Version="4.0.0" />
	</ItemGroup>
</Project>
```

- [ ] **Step 5: Write `TheHive.Api/TheHive.Api.csproj`**

```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<ImplicitUsings>enable</ImplicitUsings>
		<NeutralResourcesLanguage>en</NeutralResourcesLanguage>

		<PackageId>TheHive.Api</PackageId>
		<Owners>Panoramic Data Limited</Owners>
		<PackageProjectUrl>https://github.com/panoramicdata/TheHive.Api</PackageProjectUrl>
		<RepositoryUrl>https://github.com/panoramicdata/TheHive.Api</RepositoryUrl>
		<RepositoryType>git</RepositoryType>
		<PackageLicenseExpression>MIT</PackageLicenseExpression>
		<PackageIcon>Logo.png</PackageIcon>
		<Description>.NET 10 client for the TheHive 5 REST API.</Description>
		<PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
		<IncludeSymbols>true</IncludeSymbols>
		<SymbolPackageFormat>snupkg</SymbolPackageFormat>
		<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
		<PackageTags>TheHive;StrangeBee;SOAR;IncidentResponse;API;Client;PanoramicData</PackageTags>
		<PackageReadmeFile>README.md</PackageReadmeFile>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
		<PackageReference Include="Refit" />
		<PackageReference Include="Nerdbank.GitVersioning" PrivateAssets="all" />
	</ItemGroup>

	<ItemGroup>
		<InternalsVisibleTo Include="TheHive.Api.Test" />
	</ItemGroup>

	<ItemGroup>
		<None Include="Logo.png" Pack="true" PackagePath="" />
		<None Include="..\README.md" Link="README.md" Pack="true" PackagePath="" />
	</ItemGroup>

</Project>
```

- [ ] **Step 6: Write the test project, runner config, and a smoke test**

`TheHive.Api.Test/TheHive.Api.Test.csproj`:
```xml
<Project Sdk="Microsoft.NET.Sdk">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<OutputType>Exe</OutputType>
		<ImplicitUsings>enable</ImplicitUsings>
		<IsPackable>false</IsPackable>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="AwesomeAssertions" />
		<PackageReference Include="AwesomeAssertions.Analyzers">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
		</PackageReference>
		<PackageReference Include="Microsoft.NET.Test.Sdk" />
		<PackageReference Include="xunit.v3" />
		<PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" PrivateAssets="all" />
	</ItemGroup>

	<ItemGroup>
		<Content Include="xunit.runner.json" CopyToOutputDirectory="PreserveNewest" />
	</ItemGroup>

	<ItemGroup>
		<Using Include="Xunit" />
		<Using Include="AwesomeAssertions" />
		<ProjectReference Include="..\TheHive.Api\TheHive.Api.csproj" />
	</ItemGroup>

</Project>
```
`TheHive.Api.Test/xunit.runner.json`:
```json
{
	"$schema": "https://xunit.net/schema/current/xunit.runner.schema.json",
	"failSkips": true
}
```
`TheHive.Api.Test/SmokeTests.cs`:
```csharp
namespace TheHive.Api.Test;

public class SmokeTests
{
	[Fact]
	public void Assembly_Loads() => typeof(TheHive.Api.TheHiveMarker).Assembly.Should().NotBeNull();
}
```
`TheHive.Api/TheHiveMarker.cs` (temporary, deleted in Task 5 once `TheHiveClient` exists):
```csharp
namespace TheHive.Api;

/// <summary>Temporary assembly marker, removed once the client exists.</summary>
public static class TheHiveMarker;
```

- [ ] **Step 7: Create `TheHive.Api.slnx`, README stub, CONTRIBUTING, SECURITY, Publish.ps1**

`TheHive.Api.slnx`:
```xml
<Solution>
	<Project Path="TheHive.Api/TheHive.Api.csproj" />
	<Project Path="TheHive.Api.Test/TheHive.Api.Test.csproj" />
</Solution>
```
Copy `CONTRIBUTING.md`, `SECURITY.md`, `Publish.ps1` from `..\Highlight.Api\` and replace `Highlight` with `TheHive` throughout. `README.md`: title, one-paragraph description, install line `dotnet add package TheHive.Api`, "Status: under construction". (Full README is Task 12.)

- [ ] **Step 8: Build and test**

Run: `dotnet build TheHive.Api/TheHive.Api.csproj` -> 0 warnings, 0 errors.
Run: `dotnet test --project TheHive.Api.Test` -> 1 passed, exit 0.

- [ ] **Step 9: Commit**

```bash
git add -A
git commit -m "chore: repository skeleton, governance files, spec and plan" -m "Co-Authored-By: Claude Sonnet 5.5 <noreply@anthropic.com>"
```

---

### Task 2: CI workflow, coverage gate and Codacy config

**Files:**
- Create: `.github/workflows/ci.yml`, `.github/copilot-instructions.md`, `coverage.settings.xml`, `scripts/Check-Coverage.ps1`, `.codacy.yaml`

**Interfaces:**
- Produces: `scripts/Check-Coverage.ps1 -File <cobertura.xml> -Threshold 100` exiting non-zero below threshold; used by CI and by every later task's coverage check.

- [ ] **Step 1: Write `scripts/Check-Coverage.ps1`**

```powershell
param(
	[Parameter(Mandatory)][string]$File,
	[double]$Threshold = 100
)

[xml]$xml = Get-Content -Raw $File
$line = [double]$xml.coverage.'line-rate' * 100
$branch = [double]$xml.coverage.'branch-rate' * 100
Write-Host ("Line coverage:   {0:N2}%" -f $line)
Write-Host ("Branch coverage: {0:N2}%" -f $branch)
if ($line -lt $Threshold -or $branch -lt $Threshold) {
	Write-Error "Coverage is below the required $Threshold%."
	exit 1
}
```

- [ ] **Step 2: Write `coverage.settings.xml`** (include generated Refit stubs; they are exercised by the endpoint tests, so nothing is excluded)

```xml
<?xml version="1.0" encoding="utf-8"?>
<Configuration>
	<CodeCoverage>
		<ModulePaths>
			<Include>
				<ModulePath>.*TheHive\.Api\.dll$</ModulePath>
			</Include>
		</ModulePaths>
	</CodeCoverage>
</Configuration>
```

- [ ] **Step 3: Write `.codacy.yaml`** (no source exclusions)

```yaml
---
exclude_paths:
  - "**/bin/**"
  - "**/obj/**"
  - "TheHive.Api.Test.Integration/**"
```

- [ ] **Step 4: Write `ci.yml`.** Copy `..\Highlight.Api\.github\workflows\ci.yml`, replace `Highlight` with `TheHive`, then ensure it has these steps in the build job (add any missing): `actions/checkout@v4` with `fetch-depth: 0`; setup .NET 10; `dotnet restore`; `dotnet build -c Release --no-restore`; then

```yaml
      - name: Test with coverage
        run: dotnet test --project TheHive.Api.Test -c Release --no-build --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --coverage-settings coverage.settings.xml
      - name: Enforce 100% coverage
        shell: pwsh
        run: ./scripts/Check-Coverage.ps1 -File (Get-ChildItem -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1).FullName
```
then pack, upload `.nupkg`/`.snupkg`, and keep the tag-only publish job with `NuGet/login@v1`. Do not add the integration project to the test step.

- [ ] **Step 5: Write `.github/copilot-instructions.md`.** Copy this repo's NugetManagement version (`..\PanoramicData.NugetManagement\.github\copilot-instructions.md`) verbatim.

- [ ] **Step 6: Verify locally**

Run: `dotnet test --project TheHive.Api.Test --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --coverage-settings coverage.settings.xml` then `pwsh scripts/Check-Coverage.ps1 -File <path to the xml>`.
Expected: marker class has no executable lines, so the gate passes at 100%. If the XML is not found, search `TheHive.Api.Test/bin` for `coverage.cobertura.xml`.

- [ ] **Step 7: Commit** (`ci: coverage gate, workflow and Codacy config`)

---

### Task 3: Obtain the OpenAPI spec and build the endpoint inventory

**Files:**
- Create: `docs/endpoint-coverage.md`, `docs/openapi/thehive-5.6.json` (if obtainable)

**Interfaces:**
- Produces: `docs/endpoint-coverage.md`, a table `| Group | Method | Path | Client method | Test |` with one row per spec operation and the last two columns blank until implemented. Tasks 6-9 consume and fill it.

- [ ] **Step 1: Try to fetch the spec** (check in order, stop at the first success)

```bash
gh api repos/StrangeBeeCorp/thehive4go/contents/scripts/download_openapi.sh -q .content | base64 -d
```
Read the script for the spec URL it downloads. If it needs a running TheHive (`/api/v1/docs` style), check whether the OPS-157846 environment is ready (ask the user). Otherwise take the inventory from `thehive4go`'s generated `api/openapi.yaml` (`gh api repos/StrangeBeeCorp/thehive4go/git/trees/main?recursive=1` to find it).

- [ ] **Step 2: Save the spec** under `docs/openapi/` (JSON or YAML as obtained) and record its source URL and version in the header of `docs/endpoint-coverage.md`.

- [ ] **Step 3: Generate the inventory.** Write a throwaway `pwsh` (or `dotnet` file-based) script in the session scratchpad, not the repo, that lists `paths` x methods x tags into the markdown table, grouped by tag. Review the groups against the spec's Group list; reconcile any group names that differ (use the spec's tag name, singularised as the interface name).

- [ ] **Step 4: Commit** (`docs: OpenAPI spec and endpoint inventory`)

---

### Task 4: Core serialisation: converters and `TheHiveJson`

**Files:**
- Create: `TheHive.Api/Converters/EpochMillisecondsConverter.cs`, `TheHive.Api/Converters/TolerantEnumConverterFactory.cs`, `TheHive.Api/TheHiveJson.cs`
- Test: `TheHive.Api.Test/Core/EpochMillisecondsConverterTests.cs`, `TolerantEnumConverterTests.cs`, `TheHiveJsonTests.cs`

**Interfaces:**
- Produces: `public static class TheHiveJson { public static JsonSerializerOptions Options { get; } }` (camelCase naming off: wire names are explicit; ignores null on write; case-insensitive read; includes both converters).
- Produces: `EpochMillisecondsConverter : JsonConverter<DateTimeOffset>` (public), `TolerantEnumConverterFactory : JsonConverterFactory` (public).

- [ ] **Step 1: Write failing tests**

```csharp
using System.Text.Json;
using TheHive.Api.Converters;

namespace TheHive.Api.Test.Core;

public class EpochMillisecondsConverterTests
{
	private static readonly JsonSerializerOptions Options = new() { Converters = { new EpochMillisecondsConverter() } };

	[Fact]
	public void Read_Number_ReturnsUtcInstant() =>
		JsonSerializer.Deserialize<DateTimeOffset>("1700000000123", Options)
			.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000000123));

	[Fact]
	public void Read_NullableNull_ReturnsNull() =>
		JsonSerializer.Deserialize<DateTimeOffset?>("null", Options).Should().BeNull();

	[Fact]
	public void Read_String_Throws()
	{
		var act = () => JsonSerializer.Deserialize<DateTimeOffset>("\"x\"", Options);
		act.Should().Throw<JsonException>();
	}

	[Fact]
	public void Write_RoundTripsMilliseconds() =>
		JsonSerializer.Serialize(DateTimeOffset.FromUnixTimeMilliseconds(1700000000123), Options)
			.Should().Be("1700000000123");
}
```
```csharp
using System.Text.Json;
using TheHive.Api.Converters;

namespace TheHive.Api.Test.Core;

public class TolerantEnumConverterTests
{
	public enum Colour { Unknown = 0, Red, DarkBlue }

	private static readonly JsonSerializerOptions Options = new() { Converters = { new TolerantEnumConverterFactory() } };

	[Fact]
	public void Read_KnownValue_IsCaseInsensitive() =>
		JsonSerializer.Deserialize<Colour>("\"darkblue\"", Options).Should().Be(Colour.DarkBlue);

	[Fact]
	public void Read_UnknownValue_ReturnsDefault() =>
		JsonSerializer.Deserialize<Colour>("\"Mauve\"", Options).Should().Be(Colour.Unknown);

	[Fact]
	public void Read_NonString_Throws()
	{
		var act = () => JsonSerializer.Deserialize<Colour>("5", Options);
		act.Should().Throw<JsonException>();
	}

	[Fact]
	public void Write_UsesName() =>
		JsonSerializer.Serialize(Colour.Red, Options).Should().Be("\"Red\"");

	[Fact]
	public void CanConvert_OnlyEnums()
	{
		var factory = new TolerantEnumConverterFactory();
		factory.CanConvert(typeof(Colour)).Should().BeTrue();
		factory.CanConvert(typeof(string)).Should().BeFalse();
	}
}
```
`TheHiveJsonTests`: one test deserialising `{"x":1700000000000,"y":"Red"}` into a small record using `TheHiveJson.Options`, and one asserting a null property is omitted on write.

- [ ] **Step 2: Run, expect compile failure** (`dotnet test --project TheHive.Api.Test`; exit non-zero, types missing).

- [ ] **Step 3: Implement**

```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Converters;

/// <summary>Converts TheHive epoch-millisecond timestamps to and from <see cref="DateTimeOffset"/>.</summary>
public sealed class EpochMillisecondsConverter : JsonConverter<DateTimeOffset>
{
	/// <inheritdoc />
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		=> reader.TokenType == JsonTokenType.Number
			? DateTimeOffset.FromUnixTimeMilliseconds(reader.GetInt64())
			: throw new JsonException("Expected an epoch-millisecond number.");

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
		=> writer.WriteNumberValue(value.ToUnixTimeMilliseconds());
}
```
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheHive.Api.Converters;

/// <summary>Reads enums case-insensitively, mapping unrecognised names to the default value (<c>Unknown</c>).</summary>
public sealed class TolerantEnumConverterFactory : JsonConverterFactory
{
	/// <inheritdoc />
	public override bool CanConvert(Type typeToConvert) => typeToConvert.IsEnum;

	/// <inheritdoc />
	public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
		=> (JsonConverter)Activator.CreateInstance(typeof(TolerantEnumConverter<>).MakeGenericType(typeToConvert))!;

	private sealed class TolerantEnumConverter<T> : JsonConverter<T> where T : struct, Enum
	{
		public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			=> reader.TokenType == JsonTokenType.String
				? (Enum.TryParse<T>(reader.GetString(), true, out var value) ? value : default)
				: throw new JsonException("Expected an enum name string.");

		public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
			=> writer.WriteStringValue(value.ToString());
	}
}
```
```csharp
using System.Text.Json;
using System.Text.Json.Serialization;
using TheHive.Api.Converters;

namespace TheHive.Api;

/// <summary>Shared <see cref="JsonSerializerOptions"/> for TheHive payloads.</summary>
public static class TheHiveJson
{
	/// <summary>The options used by <see cref="TheHiveClient"/>.</summary>
	public static JsonSerializerOptions Options { get; } = Create();

	private static JsonSerializerOptions Create()
	{
		var options = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
		};
		options.Converters.Add(new EpochMillisecondsConverter());
		options.Converters.Add(new TolerantEnumConverterFactory());
		return options;
	}
}
```

- [ ] **Step 4: Run tests, expect all pass.** Check coverage of the three new files is 100% (branch included); add tests for any gap.

- [ ] **Step 5: Commit** (`feat: epoch and tolerant-enum JSON converters`)

---

### Task 5: Options, exception, error mapper, retry handler, client shell

**Files:**
- Create: `TheHiveClientOptions.cs`, `TheHiveApiException.cs`, `TheHiveErrorMapper.cs`, `Handlers/AuthRetryHandler.cs`, `TheHiveClient.cs`
- Delete: `TheHive.Api/TheHiveMarker.cs`, `TheHive.Api.Test/SmokeTests.cs`
- Create tests: `Core/OptionsTests.cs`, `Core/ErrorMapperTests.cs`, `Core/AuthRetryHandlerTests.cs`, `Core/ClientTests.cs`, `Support/StubHandler.cs`, `Support/TestClient.cs`

**Interfaces:**
- Produces: `TheHiveClientOptions` (`string BaseUrl`, `string ApiKey`, `string? Organisation`, `TimeSpan Timeout`, `int MaxRetries`, `TimeSpan RetryBaseDelay`, `ILogger? Logger`; `ToString()` masks the key; `internal void Validate()` throws `ArgumentException`/`ArgumentOutOfRangeException`).
- Produces: `TheHiveApiException(HttpStatusCode statusCode, string? errorType, string message, string? requestId)` with get-only properties `StatusCode`, `ErrorType`, `RequestId`.
- Produces: `internal static class TheHiveErrorMapper { Task<Exception?> CreateAsync(HttpResponseMessage) }`.
- Produces: `internal sealed class AuthRetryHandler(TheHiveClientOptions options) : DelegatingHandler` with `internal Func<TimeSpan, CancellationToken, Task> Delay` defaulting to `Task.Delay`.
- Produces: `TheHiveClient(TheHiveClientOptions)` public; `internal TheHiveClient(TheHiveClientOptions, HttpMessageHandler inner)`; `IDisposable`. Group properties are added by Tasks 6-9.
- Produces (tests): `StubHandler` (`Enqueue(HttpStatusCode, string json = "{}", Action<HttpResponseMessage>? configure = null)`, `Calls` = list of `RecordedCall(HttpMethod Method, Uri Uri, string? Body, HttpRequestHeaders Headers)`) and `TestClient.Create(StubHandler, Action<TheHiveClientOptions>? tweak = null)` returning a `TheHiveClient` with `BaseUrl = "https://hive.test/"`, `ApiKey = "secret-key"`, `MaxRetries = 0`.

- [ ] **Step 1: Write the support types and failing tests.** `StubHandler`:

```csharp
using System.Net;
using System.Net.Http.Headers;

namespace TheHive.Api.Test.Support;

internal sealed record RecordedCall(HttpMethod Method, Uri Uri, string? Body, HttpRequestHeaders Headers);

internal sealed class StubHandler : HttpMessageHandler
{
	private readonly Queue<Func<HttpResponseMessage>> _responses = new();

	public List<RecordedCall> Calls { get; } = [];

	public void Enqueue(HttpStatusCode status, string json = "{}", Action<HttpResponseMessage>? configure = null)
		=> _responses.Enqueue(() =>
		{
			var response = new HttpResponseMessage(status)
			{
				Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json")
			};
			configure?.Invoke(response);
			return response;
		});

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var body = request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken);
		Calls.Add(new RecordedCall(request.Method, request.RequestUri!, body, request.Headers));
		return _responses.Dequeue()();
	}
}
```
`TestClient.Create` builds options and calls the internal constructor `new TheHiveClient(options, stub)`.

Tests to write (names are the assertions; each is its own `[Fact]`/`[Theory]`):
- Options: empty `BaseUrl` throws; relative `BaseUrl` throws; empty `ApiKey` throws; `MaxRetries < 0` throws; `Timeout <= 0` throws; valid options do not throw; `ToString()` does not contain the key.
- ErrorMapper: success response -> null; JSON `{"type":"NotFound","message":"nope"}` with 404 -> exception with those fields; `X-Request-Id` header -> `RequestId`; HTML body with 502 -> message falls back to `"HTTP 502 (Bad Gateway)"`; empty body -> fallback message; JSON without `message` -> fallback.
- Handler (set `handler.Delay` to a recording lambda returning `Task.CompletedTask`): adds `Authorization: Bearer secret-key`; adds `X-Organisation` only when configured; 503 then 200 with `MaxRetries=2` -> 2 calls, one delay of `RetryBaseDelay`; second retry delay is double the first; 429 with `Retry-After: 7` delta -> delay is 7 s; 429 with `Retry-After` HTTP date (now+10 s, via `RetryConditionHeaderValue`) -> delay is positive and <= 10 s; 429 with no header uses back-off; persistent 500 with `MaxRetries=1` -> returns the 500 after 2 calls; 400/404 are not retried; a delay that throws `OperationCanceledException` propagates; logger (a capturing `ILogger` fake) receives messages and none contain `secret-key`; a null logger does not throw.
- Client: `BaseUrl = "https://hive.test/thehive"` (no trailing slash) resolves a relative path under `/thehive/`; the public constructor builds without throwing; `Dispose()` twice does not throw.

- [ ] **Step 2: Run tests, expect compile failure.**

- [ ] **Step 3: Implement.**

```csharp
using Microsoft.Extensions.Logging;

namespace TheHive.Api;

/// <summary>Configuration for <see cref="TheHiveClient"/>.</summary>
public class TheHiveClientOptions
{
	/// <summary>Absolute URL of the TheHive instance, e.g. <c>https://thehive.example.com</c>.</summary>
	public string BaseUrl { get; set; } = string.Empty;

	/// <summary>The API key sent as a bearer token.</summary>
	public string ApiKey { get; set; } = string.Empty;

	/// <summary>Optional organisation name, sent as <c>X-Organisation</c>.</summary>
	public string? Organisation { get; set; }

	/// <summary>HTTP timeout per attempt.</summary>
	public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

	/// <summary>Maximum retries for 429 and 5xx responses.</summary>
	public int MaxRetries { get; set; } = 3;

	/// <summary>Initial back-off, doubled on each retry.</summary>
	public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

	/// <summary>Optional logger. The API key is never logged.</summary>
	public ILogger? Logger { get; set; }

	internal void Validate()
	{
		if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
		{
			throw new ArgumentException("BaseUrl must be an absolute URL.", nameof(BaseUrl));
		}

		ArgumentException.ThrowIfNullOrWhiteSpace(ApiKey);
		ArgumentOutOfRangeException.ThrowIfNegative(MaxRetries);
		ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(Timeout, TimeSpan.Zero);
	}

	/// <inheritdoc />
	public override string ToString() => $"TheHiveClientOptions {{ BaseUrl = {BaseUrl}, ApiKey = ***, Organisation = {Organisation} }}";
}
```
```csharp
using System.Net;

namespace TheHive.Api;

/// <summary>Raised when TheHive returns a non-success response.</summary>
public sealed class TheHiveApiException(HttpStatusCode statusCode, string? errorType, string message, string? requestId)
	: Exception(message)
{
	/// <summary>The HTTP status code.</summary>
	public HttpStatusCode StatusCode { get; } = statusCode;

	/// <summary>TheHive's error <c>type</c>, when supplied.</summary>
	public string? ErrorType { get; } = errorType;

	/// <summary>The <c>X-Request-Id</c> response header, when supplied.</summary>
	public string? RequestId { get; } = requestId;
}
```
```csharp
using System.Text.Json;

namespace TheHive.Api;

internal static class TheHiveErrorMapper
{
	public static async Task<Exception?> CreateAsync(HttpResponseMessage response)
	{
		if (response.IsSuccessStatusCode)
		{
			return null;
		}

		string? type = null;
		string? message = null;
		var body = response.Content is null ? string.Empty : await response.Content.ReadAsStringAsync();
		try
		{
			using var document = JsonDocument.Parse(body);
			if (document.RootElement.ValueKind == JsonValueKind.Object)
			{
				type = document.RootElement.TryGetProperty("type", out var t) ? t.GetString() : null;
				message = document.RootElement.TryGetProperty("message", out var m) ? m.GetString() : null;
			}
		}
		catch (JsonException)
		{
			// Non-JSON body (e.g. a proxy error page): fall through to the fallback message.
		}

		var requestId = response.Headers.TryGetValues("X-Request-Id", out var values) ? values.FirstOrDefault() : null;
		return new TheHiveApiException(
			response.StatusCode,
			type,
			message ?? $"HTTP {(int)response.StatusCode} ({response.ReasonPhrase ?? response.StatusCode.ToString()})",
			requestId);
	}
}
```
In the 502 test, the stub's `HttpResponseMessage` has default `ReasonPhrase` "Bad Gateway", matching the fallback text.

```csharp
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace TheHive.Api.Handlers;

internal sealed class AuthRetryHandler(TheHiveClientOptions options) : DelegatingHandler
{
	internal Func<TimeSpan, CancellationToken, Task> Delay { get; set; } = Task.Delay;

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", options.ApiKey);
		if (!string.IsNullOrWhiteSpace(options.Organisation))
		{
			request.Headers.Remove("X-Organisation");
			request.Headers.Add("X-Organisation", options.Organisation);
		}

		var backoff = options.RetryBaseDelay;
		for (var attempt = 0; ; attempt++)
		{
			options.Logger?.LogDebug("TheHive {Method} {Uri} (attempt {Attempt})", request.Method, request.RequestUri, attempt + 1);
			var response = await base.SendAsync(request, cancellationToken);
			if (!IsTransient(response.StatusCode) || attempt >= options.MaxRetries)
			{
				return response;
			}

			var wait = RetryAfter(response) ?? backoff;
			options.Logger?.LogWarning("TheHive returned {Status}; retrying in {Delay}", (int)response.StatusCode, wait);
			response.Dispose();
			await Delay(wait, cancellationToken);
			backoff *= 2;
		}
	}

	private static bool IsTransient(HttpStatusCode status) => status == HttpStatusCode.TooManyRequests || (int)status >= 500;

	private static TimeSpan? RetryAfter(HttpResponseMessage response)
	{
		var header = response.Headers.RetryAfter;
		if (header?.Delta is { } delta)
		{
			return delta;
		}

		if (header?.Date is { } date)
		{
			var wait = date - DateTimeOffset.UtcNow;
			return wait > TimeSpan.Zero ? wait : TimeSpan.Zero;
		}

		return null;
	}
}
```
Note: `request` is reused across attempts; this is safe for the buffered `StringContent` that Refit produces. Add a handler test with a POST body and a retry to prove the body is re-sent.

```csharp
using Refit;

namespace TheHive.Api;

/// <summary>Client for the TheHive 5 REST API.</summary>
public sealed class TheHiveClient : IDisposable
{
	private readonly HttpClient _httpClient;

	/// <summary>Creates a client.</summary>
	/// <param name="options">Connection options.</param>
	public TheHiveClient(TheHiveClientOptions options) : this(options, new HttpClientHandler())
	{
	}

	internal TheHiveClient(TheHiveClientOptions options, HttpMessageHandler inner)
	{
		options.Validate();
		var baseUrl = options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/";
		_httpClient = new HttpClient(new Handlers.AuthRetryHandler(options) { InnerHandler = inner })
		{
			BaseAddress = new Uri(baseUrl),
			Timeout = options.Timeout
		};
		var settings = new RefitSettings
		{
			ContentSerializer = new SystemTextJsonContentSerializer(TheHiveJson.Options),
			ExceptionFactory = TheHiveErrorMapper.CreateAsync
		};
		_ = settings; // group properties are created from these settings in Tasks 6-9
	}

	/// <inheritdoc />
	public void Dispose() => _httpClient.Dispose();
}
```
Refit paths must be **relative without a leading slash** (`"api/v1/case"`) so a base path prefix is preserved. Store `settings` in a private field `_settings` from Task 6 onward and remove the discard line then.

- [ ] **Step 4: Run tests green; confirm 100% line+branch on all Task 5 files.** The `attempt >= MaxRetries` and `wait > Zero` branches each need a test (see list above).

- [ ] **Step 5: Delete `TheHiveMarker.cs` and `SmokeTests.cs`, rebuild, rerun, commit** (`feat: options, error mapping, retry handler and client shell`).

---

### Task 6: Cases group (worked example for the Group Recipe)

**Files:**
- Create: `Interfaces/ICases.cs`, `Data/Cases/Case.cs`, `Data/Cases/CaseCreateRequest.cs`, `Data/Cases/CaseUpdateRequest.cs`, `Data/Cases/CaseStatus.cs`, `Data/Cases/Tlp.cs` (shared enums `Tlp`, `Pap` go in `Data/Common/`)
- Modify: `TheHiveClient.cs` (add `public ICases Cases { get; }`, store settings in `_settings`)
- Test: `TheHive.Api.Test/Groups/CasesTests.cs`

**Interfaces:**
- Consumes: `TestClient.Create`, `StubHandler`, `TheHiveJson.Options`.
- Produces: `ICases` with
  - `Task<Case> CreateAsync(CaseCreateRequest request, CancellationToken cancellationToken = default)` -> `POST api/v1/case`
  - `Task<Case> GetAsync(string id, CancellationToken cancellationToken = default)` -> `GET api/v1/case/{id}`
  - `Task UpdateAsync(string id, CaseUpdateRequest request, CancellationToken cancellationToken = default)` -> `PATCH api/v1/case/{id}`
  - `Task DeleteAsync(string id, CancellationToken cancellationToken = default)` -> `DELETE api/v1/case/{id}`
  - `Task<Case> MergeAsync(string id, string otherId, CancellationToken cancellationToken = default)` -> `POST api/v1/case/{id}/merge/{otherId}`
- Produces: `Case` model; `CaseStatus` enum `{ Unknown, New, InProgress, Resolved, Closed }` (final value set confirmed against spec in Task 3); `Tlp` `{ Unknown, Clear, Green, Amber, AmberStrict, Red }` etc.

- [ ] **Step 1: Write the failing tests**

```csharp
using System.Net;
using TheHive.Api.Data.Cases;
using TheHive.Api.Test.Support;

namespace TheHive.Api.Test.Groups;

public class CasesTests
{
	private const string CaseJson = """
		{"_id":"~123","_type":"Case","_createdAt":1700000000000,"number":7,"title":"Phish","description":"d",
		 "severity":3,"status":"InProgress","tags":["a","b"],"flag":true,"tlp":2,"pap":2,"assignee":"bob"}
		""";

	[Fact]
	public async Task GetAsync_MapsCase()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.GetAsync("~123");

		stub.Calls.Should().ContainSingle();
		stub.Calls[0].Method.Should().Be(HttpMethod.Get);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
		result.Id.Should().Be("~123");
		result.Number.Should().Be(7);
		result.Title.Should().Be("Phish");
		result.Status.Should().Be(CaseStatus.InProgress);
		result.Tags.Should().Equal("a", "b");
		result.CreatedAt.Should().Be(DateTimeOffset.FromUnixTimeMilliseconds(1700000000000));
	}

	[Fact]
	public async Task CreateAsync_PostsBodyAndMapsResult()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.Created, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.CreateAsync(new CaseCreateRequest { Title = "Phish", Description = "d", Severity = 3 });

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case");
		stub.Calls[0].Body.Should().Contain("\"title\":\"Phish\"").And.Contain("\"severity\":3");
		result.Number.Should().Be(7);
	}

	[Fact]
	public async Task UpdateAsync_PatchesBody()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.UpdateAsync("~123", new CaseUpdateRequest { Title = "New" });

		stub.Calls[0].Method.Should().Be(HttpMethod.Patch);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
		stub.Calls[0].Body.Should().Be("{\"title\":\"New\"}");
	}

	[Fact]
	public async Task DeleteAsync_SendsDelete()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NoContent, "");
		using var client = TestClient.Create(stub);

		await client.Cases.DeleteAsync("~123");

		stub.Calls[0].Method.Should().Be(HttpMethod.Delete);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~123");
	}

	[Fact]
	public async Task MergeAsync_PostsToMergePath()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.OK, CaseJson);
		using var client = TestClient.Create(stub);

		var result = await client.Cases.MergeAsync("~1", "~2");

		stub.Calls[0].Method.Should().Be(HttpMethod.Post);
		stub.Calls[0].Uri.AbsolutePath.Should().Be("/api/v1/case/~1/merge/~2");
		result.Id.Should().Be("~123");
	}

	[Fact]
	public async Task GetAsync_NotFound_ThrowsTheHiveApiException()
	{
		var stub = new StubHandler();
		stub.Enqueue(HttpStatusCode.NotFound, """{"type":"NotFoundError","message":"Case not found"}""");
		using var client = TestClient.Create(stub);

		var act = () => client.Cases.GetAsync("~missing");

		(await act.Should().ThrowAsync<TheHiveApiException>())
			.Which.Should().Match<TheHiveApiException>(e => e.StatusCode == HttpStatusCode.NotFound && e.ErrorType == "NotFoundError");
	}
}
```
Also add one test that an unknown `status` string (`"Archived2099"`) yields `CaseStatus.Unknown`, and that an absent `endDate` maps to `null`.

- [ ] **Step 2: Run, expect compile failure.**

- [ ] **Step 3: Implement.** Shared enums (`Data/Common/Tlp.cs`, `Pap.cs`) use `Unknown = 0` and the numeric TheHive levels as **names** only if the spec transmits strings; TheHive 5 sends TLP/PAP as integers 0-4, so model them as `int` properties plus a documented helper, not enums, if the spec confirms integers (decide in Task 3 notes; the test above assumes `Tlp` is `int`: adjust the model, not the test, if the spec says otherwise). Models:

```csharp
using System.Text.Json.Serialization;

namespace TheHive.Api.Data.Cases;

/// <summary>A TheHive case.</summary>
public sealed class Case
{
	/// <summary>The case identifier.</summary>
	[JsonPropertyName("_id")] public string Id { get; set; } = string.Empty;

	/// <summary>When the case was created.</summary>
	[JsonPropertyName("_createdAt")] public DateTimeOffset CreatedAt { get; set; }

	/// <summary>The case number.</summary>
	[JsonPropertyName("number")] public int Number { get; set; }

	/// <summary>The title.</summary>
	[JsonPropertyName("title")] public string Title { get; set; } = string.Empty;

	/// <summary>The description.</summary>
	[JsonPropertyName("description")] public string Description { get; set; } = string.Empty;

	/// <summary>Severity from 1 (low) to 4 (critical).</summary>
	[JsonPropertyName("severity")] public int Severity { get; set; }

	/// <summary>The workflow status.</summary>
	[JsonPropertyName("status")] public CaseStatus Status { get; set; }

	/// <summary>When the case ended, if it has.</summary>
	[JsonPropertyName("endDate")] public DateTimeOffset? EndDate { get; set; }

	/// <summary>The tags.</summary>
	[JsonPropertyName("tags")] public List<string> Tags { get; set; } = [];

	/// <summary>Whether the case is flagged.</summary>
	[JsonPropertyName("flag")] public bool Flag { get; set; }

	/// <summary>Traffic Light Protocol level.</summary>
	[JsonPropertyName("tlp")] public int Tlp { get; set; }

	/// <summary>Permissible Actions Protocol level.</summary>
	[JsonPropertyName("pap")] public int Pap { get; set; }

	/// <summary>The assignee login.</summary>
	[JsonPropertyName("assignee")] public string? Assignee { get; set; }
}
```
`CaseCreateRequest`: `Title` (required string), `Description` (required string), `Severity` (`int?`), `Tags` (`List<string>?`), `Flag` (`bool?`), `Tlp`/`Pap` (`int?`), `Assignee` (`string?`), `StartDate` (`DateTimeOffset?`), each with `[JsonPropertyName]` camelCase and XML docs. `CaseUpdateRequest`: all properties nullable and null-omitted (`TheHiveJson.Options` already omits nulls, so `{"title":"New"}` is the expected body). `ICases`:

```csharp
using Refit;
using TheHive.Api.Data.Cases;

namespace TheHive.Api.Interfaces;

/// <summary>Operations on TheHive cases.</summary>
public interface ICases
{
	/// <summary>Creates a case.</summary>
	[Post("/api/v1/case")]
	Task<Case> CreateAsync([Body] CaseCreateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Gets a case by id or number.</summary>
	[Get("/api/v1/case/{id}")]
	Task<Case> GetAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Updates a case.</summary>
	[Patch("/api/v1/case/{id}")]
	Task UpdateAsync(string id, [Body] CaseUpdateRequest request, CancellationToken cancellationToken = default);

	/// <summary>Deletes a case.</summary>
	[Delete("/api/v1/case/{id}")]
	Task DeleteAsync(string id, CancellationToken cancellationToken = default);

	/// <summary>Merges <paramref name="otherId"/> into <paramref name="id"/>.</summary>
	[Post("/api/v1/case/{id}/merge/{otherId}")]
	Task<Case> MergeAsync(string id, string otherId, CancellationToken cancellationToken = default);
}
```
**Leading slash caveat:** Refit prepends `BaseAddress` to attribute paths; a leading `/` replaces the base *path* (breaking `https://host/thehive`). Therefore write attribute paths **without** a leading slash (`"api/v1/case"`) in every interface; the Review Focus item 6 test (path-prefixed base URL) will fail otherwise. Correct the snippet above accordingly when implementing, and add that test in `CasesTests`.

In `TheHiveClient`: add `private readonly RefitSettings _settings;`, `public ICases Cases { get; }`, and in the constructor `Cases = RestService.For<ICases>(_httpClient, _settings);` (remove the `_ = settings` line).

- [ ] **Step 4: Run Cases tests green; run the coverage command from Task 2 and confirm the gate passes.**

- [ ] **Step 5: Update `docs/endpoint-coverage.md` rows; commit** (`feat: add Cases endpoints`).

---

### Task 7: Query builder and Query/Export group

**Files:**
- Create: `Query/QueryStep.cs`, `Query/QuerySteps.cs`, `Query/QueryBuilder.cs`, `Interfaces/IQuery.cs`, `Data/Query/QueryRequest.cs`
- Test: `TheHive.Api.Test/Query/QueryBuilderTests.cs`, `Groups/QueryTests.cs`

**Interfaces:**
- Produces: `QueryBuilder` (fluent, immutable-per-call returning `this` is acceptable): `QueryBuilder.List(string listName)` where `listName` is the TheHive step (`"listCase"`, `"listAlert"`, `"listTask"`, `"listObservable"`...); `Filter(string field, object? value)` (`_eq`); `FilterIn(string field, IEnumerable<object> values)` (`_in`); `FilterLike`, `FilterGt`, `FilterLt`, `FilterBetween`, `FilterAnd`, `FilterOr`, `FilterNot` (nested via `Action<FilterBuilder>`); `Sort(string field, SortDirection direction)`; `Page(int from, int to)`; `Select(params string[] fields)`; `Count()`; `Build()` returning `QueryRequest { JsonArray Query }`.
- Produces: `IQuery.RunAsync(QueryRequest request, string? name, CancellationToken)` -> `POST api/v1/query` returning `JsonElement`; plus typed helper `Task<List<T>> RunAsync<T>(...)` in `QueryExtensions` that deserialises the array with `TheHiveJson.Options`.
- Wire shape (assert exactly in tests): `{"query":[{"_name":"listCase"},{"_name":"filter","_eq":{"_field":"status","_value":"New"}},{"_name":"sort","_fields":[{"title":"asc"}]},{"_name":"page","from":0,"to":15}]}`

- [ ] **Step 1: Write failing tests** asserting `Build().ToJsonString()` equals the exact strings for: list only; list+eq filter; `_in`; `_like`; `_gt`/`_lt`/`_between`; nested and/or/not; sort asc/desc; page; select; count; a full pipeline equal to the wire shape above; `Page(5, 2)` throws `ArgumentOutOfRangeException`; empty field name throws `ArgumentException`; building with no list step throws `InvalidOperationException`. In `QueryTests`: `RunAsync` posts to `/api/v1/query`, with `?name=cases` when a name is given, and the typed helper maps a JSON array of `Case`.

- [ ] **Step 2: Run, expect compile failure.**

- [ ] **Step 3: Implement** `QueryBuilder` over `System.Text.Json.Nodes.JsonArray`/`JsonObject`: each method appends a `JsonObject` with `_name` and the step's fields; `Build()` validates the first step is a `list*`/`getX` step. Every public member XML-documented. Interface:

```csharp
/// <summary>Runs queries against TheHive.</summary>
public interface IQuery
{
	/// <summary>Runs a query and returns the raw JSON result.</summary>
	[Post("api/v1/query")]
	Task<JsonElement> RunAsync([Body] QueryRequest request, [Query] string? name = null, CancellationToken cancellationToken = default);
}
```
`QueryRequest` serialises to `{"query":[...]}` (`[JsonPropertyName("query")] public JsonArray Query`). Add `Query` property to `TheHiveClient`.

- [ ] **Step 4: Tests green; coverage gate passes.**

- [ ] **Step 5: Commit** (`feat: query builder and Query endpoint`).

---

### Task 8: Core incident-response groups

Apply the **Group Recipe** to each group below as its own commit. Operations listed are the minimum; Task 3's inventory is authoritative and may add more. Paths are relative (no leading slash). Every method takes a trailing `CancellationToken`.

| Group (interface, property) | Operations (client method -> verb path) |
|---|---|
| Alerts (`IAlerts`, `Alerts`) | `CreateAsync` POST `api/v1/alert`; `GetAsync` GET `api/v1/alert/{id}`; `UpdateAsync` PATCH `api/v1/alert/{id}`; `DeleteAsync` DELETE `api/v1/alert/{id}`; `PromoteToCaseAsync` POST `api/v1/alert/{id}/case`; `MergeIntoCaseAsync` POST `api/v1/alert/{id}/merge/{caseId}`; `FollowAsync` POST `api/v1/alert/{id}/follow`; `UnfollowAsync` POST `api/v1/alert/{id}/unfollow`; `BulkUpdateAsync` PATCH `api/v1/alert/_bulk` |
| Tasks (`ITasks`, `Tasks`) | `CreateAsync` POST `api/v1/case/{caseId}/task`; `GetAsync` GET `api/v1/task/{id}`; `UpdateAsync` PATCH `api/v1/task/{id}`; `DeleteAsync` DELETE `api/v1/task/{id}`; `ListActionRequiredAsync` GET `api/v1/task/{id}/actionRequired` |
| TaskLogs (`ITaskLogs`, `TaskLogs`) | `CreateAsync` POST `api/v1/task/{taskId}/log`; `GetAsync` GET `api/v1/task/log/{id}`; `UpdateAsync` PATCH `api/v1/task/log/{id}`; `DeleteAsync` DELETE `api/v1/task/log/{id}`; `AddAttachmentAsync` POST `api/v1/task/log/{id}/attachments` (multipart) ; `DownloadAttachmentAsync` GET `api/v1/task/log/{id}/attachment/{attachmentId}/download` |
| Observables (`IObservables`, `Observables`) | `CreateInCaseAsync` POST `api/v1/case/{caseId}/observable`; `CreateInAlertAsync` POST `api/v1/alert/{alertId}/observable`; `GetAsync` GET `api/v1/observable/{id}`; `UpdateAsync` PATCH `api/v1/observable/{id}`; `DeleteAsync` DELETE `api/v1/observable/{id}`; `BulkUpdateAsync` PATCH `api/v1/observable/_bulk`; `ShareAsync` POST `api/v1/observable/{id}/shares` |
| Comments (`IComments`, `Comments`) | `CreateOnCaseAsync` POST `api/v1/case/{caseId}/comment`; `CreateOnAlertAsync` POST `api/v1/alert/{alertId}/comment`; `UpdateAsync` PATCH `api/v1/comment/{id}`; `DeleteAsync` DELETE `api/v1/comment/{id}` |
| Attachments (`IAttachments`, `Attachments`) | `UploadToCaseAsync` POST `api/v1/case/{caseId}/attachments` (multipart); `UploadToAlertAsync` POST `api/v1/alert/{alertId}/attachments` (multipart); `DownloadAsync` GET `api/v1/case/{caseId}/attachment/{attachmentId}/download` returning `Stream`; `DeleteAsync` DELETE `api/v1/case/{caseId}/attachment/{attachmentId}` |
| CaseTemplates (`ICaseTemplates`, `CaseTemplates`) | `CreateAsync` POST `api/v1/caseTemplate`; `GetAsync` GET `api/v1/caseTemplate/{id}`; `UpdateAsync` PATCH `api/v1/caseTemplate/{id}`; `DeleteAsync` DELETE `api/v1/caseTemplate/{id}` |

Group-specific test requirements beyond the recipe:
- Multipart and stream methods (`AddAttachmentAsync`, `Upload*Async`, `DownloadAsync`): assert the request `Content-Type` is `multipart/form-data` (stub body contains the file name and bytes), and that the download returns the exact bytes with `Stream` disposed by the caller.
- `ListActionRequiredAsync` and any bulk endpoint: assert the body shape (`{"ids":[...],...}` per spec) and that an empty id list is rejected client-side with `ArgumentException` (validate in a thin default-interface-free wrapper only if the spec requires it; otherwise document that the server rejects it).

Verification per group: group tests green, `docs/endpoint-coverage.md` rows complete, coverage gate green. Commit: `feat: add {Group} endpoints`.

---

### Task 9: Administration, reference-data and remaining groups

Apply the **Group Recipe** to each group, one commit each. The operations are the minimum; Task 3's inventory is authoritative, and any further operation it lists for these groups is added.

| Group (interface, property) | Operations |
|---|---|
| CustomFields (`ICustomFields`, `CustomFields`) | `CreateAsync` POST `api/v1/customField`; `GetAsync` GET `api/v1/customField/{id}`; `UpdateAsync` PATCH `api/v1/customField/{id}`; `DeleteAsync` DELETE `api/v1/customField/{id}` |
| Users (`IUsers`, `Users`) | `CreateAsync` POST `api/v1/user`; `GetAsync` GET `api/v1/user/{id}`; `GetCurrentAsync` GET `api/v1/user/current`; `UpdateAsync` PATCH `api/v1/user/{id}`; `LockAsync` POST `api/v1/user/{id}/lock`; `UnlockAsync` POST `api/v1/user/{id}/unlock`; `SetPasswordAsync` POST `api/v1/user/{id}/password/set`; `ChangePasswordAsync` POST `api/v1/user/{id}/password/change`; `GetApiKeyAsync` GET `api/v1/user/{id}/key`; `RenewApiKeyAsync` POST `api/v1/user/{id}/key/renew`; `RemoveApiKeyAsync` DELETE `api/v1/user/{id}/key` |
| Organisations (`IOrganisations`, `Organisations`) | `CreateAsync` POST `api/v1/organisation`; `GetAsync` GET `api/v1/organisation/{id}`; `UpdateAsync` PATCH `api/v1/organisation/{id}`; `BulkLinkAsync` PUT `api/v1/organisation/{id}/links` |
| Profiles (`IProfiles`, `Profiles`) | `CreateAsync` POST `api/v1/profile`; `GetAsync` GET `api/v1/profile/{id}`; `UpdateAsync` PATCH `api/v1/profile/{id}`; `DeleteAsync` DELETE `api/v1/profile/{id}` |
| Procedures (`IProcedures`, `Procedures`) | `CreateAsync` POST `api/v1/procedure`; `GetAsync` GET `api/v1/procedure/{id}`; `DeleteAsync` DELETE `api/v1/procedure/{id}` |
| Patterns (`IPatterns`, `Patterns`) | `ImportAsync` POST `api/v1/pattern/import/attack` (multipart); `GetAsync` GET `api/v1/pattern/{id}`; `DeleteAsync` DELETE `api/v1/pattern/{id}` |
| Timeline (`ITimeline`, `Timeline`) | `GetCaseTimelineAsync` GET `api/v1/case/{caseId}/timeline`; `CreateEventAsync` POST `api/v1/case/{caseId}/timeline`; `UpdateEventAsync` PATCH `api/v1/case/{caseId}/timeline/{eventId}`; `DeleteEventAsync` DELETE `api/v1/case/{caseId}/timeline/{eventId}` |
| Dashboards (`IDashboards`, `Dashboards`) | `CreateAsync` POST `api/v1/dashboard`; `GetAsync` GET `api/v1/dashboard/{id}`; `UpdateAsync` PATCH `api/v1/dashboard/{id}`; `DeleteAsync` DELETE `api/v1/dashboard/{id}` |
| Cortex (`ICortex`, `Cortex`) | `ListAnalyzersAsync` GET `api/connector/cortex/analyzer`; `RunAnalyzerAsync` POST `api/connector/cortex/job`; `GetJobAsync` GET `api/connector/cortex/job/{id}`; `ListResponders`/`RunResponder` per the spec |
| Admin (`IAdmin`, `Admin`) | `GetStatusAsync` GET `api/status`; `GetVersionAsync` GET `api/v1/status` (whichever the spec names the status operation); `GetPermissionsAsync` GET `api/v1/permission`; `ListAuditAsync`, `ListTagsAsync`, `ListPagesAsync` and the remaining admin operations exactly as the spec's Admin and Audit tags list them |

Rows whose path or verb I could not recall with certainty are flagged `(verify)` in `docs/endpoint-coverage.md` and corrected against the spec before their tests are written; **the spec wins over this table**. Password and API-key operations additionally need a test that the secret never appears in `TheHiveApiException.Message` or logs.

Verification after the final group: `dotnet test --project TheHive.Api.Test --coverage ...` + `Check-Coverage.ps1` at 100%; the completeness test in Task 10 passes.

---

### Task 10: Completeness and structure tests

**Files:**
- Create: `TheHive.Api.Test/Core/StructureTests.cs`

- [ ] **Step 1: Write the tests**

```csharp
using System.Reflection;
using Refit;
using TheHive.Api.Interfaces;

namespace TheHive.Api.Test.Core;

public class StructureTests
{
	private static IEnumerable<Type> Interfaces => typeof(TheHiveClient).Assembly.GetTypes()
		.Where(t => t.IsInterface && t.Namespace == typeof(IQuery).Namespace);

	[Fact]
	public void EveryInterface_IsPublic() =>
		Interfaces.Should().OnlyContain(t => t.IsPublic);

	[Fact]
	public void EveryMethod_HasARefitVerbAndRelativePath()
	{
		foreach (var method in Interfaces.SelectMany(t => t.GetMethods()))
		{
			var http = method.GetCustomAttributes().OfType<HttpMethodAttribute>().Single();
			http.Path.Should().NotStartWith("/", $"{method.DeclaringType!.Name}.{method.Name} must use a relative path");
		}
	}

	[Fact]
	public void EveryMethod_ReturnsTaskAndEndsWithCancellationToken()
	{
		foreach (var method in Interfaces.SelectMany(t => t.GetMethods()))
		{
			typeof(Task).IsAssignableFrom(method.ReturnType).Should().BeTrue(method.Name);
			method.GetParameters().Last().ParameterType.Should().Be<CancellationToken>(method.Name);
		}
	}

	[Fact]
	public void EveryInterface_HasAMatchingClientProperty()
	{
		var properties = typeof(TheHiveClient).GetProperties().Select(p => p.PropertyType).ToList();
		Interfaces.Should().OnlyContain(i => properties.Contains(i));
	}
}
```
- [ ] **Step 2: Run, fix any interface that violates these rules, run green, commit** (`test: structural guards for the Refit interfaces`).

---

### Task 11: Coverage and Codacy sweep

**Files:** whatever the sweep touches.

- [ ] **Step 1: Coverage.** Run the coverage command; `Check-Coverage.ps1` must report 100.00% line and branch. For each uncovered line or branch in the cobertura XML, add a test that exercises it (never an exclusion, never `[ExcludeFromCodeCoverage]`).

- [ ] **Step 2: Local analysis.** `dotnet build TheHive.Api/TheHive.Api.csproj -c Release` -> 0 warnings. Apply `dotnet format` only to files you changed; fix any `IDE0055`.

- [ ] **Step 3: Add the repo to Codacy and read the issue count.** Use the Codacy token via the Yoda broker over native messaging (see memory `codacy-token-from-keepassxc`; never `keepassxc-cli`). If the harness blocks this, stop and ask the user to add `panoramicdata/TheHive.Api` to Codacy and supply the issue list; do not guess. Query `https://app.codacy.com/api/v3/analysis/organizations/gh/panoramicdata/repositories/TheHive.Api/issues/search` (POST) and fix every issue at the source. Zero issues is the gate; do not suppress.

- [ ] **Step 4: Commit** (`chore: coverage and Codacy sweep`) and push the branch; open a PR with `gh pr create` (description ends with the Claude Code attribution line).

---

### Task 12: Documentation and packaging

**Files:**
- Modify: `README.md`, `CONTRIBUTING.md`
- Create: `PUBLISHING.md` (adapt Highlight.Api's if present)

- [ ] **Step 1: README.** Badges (NuGet version, MIT, Codacy grade + coverage, .NET 10); install; quick start:

```csharp
using var client = new TheHiveClient(new TheHiveClientOptions
{
	BaseUrl = "https://thehive.example.com",
	ApiKey = Environment.GetEnvironmentVariable("THEHIVE_API_KEY")!,
	Organisation = "my-org"
});

var created = await client.Cases.CreateAsync(new CaseCreateRequest { Title = "Phish", Description = "Reported by finance" });
var open = await client.Query.RunAsync<Case>(QueryBuilder.List("listCase").Filter("status", "New").Sort("_createdAt", SortDirection.Descending).Page(0, 25));
```
Add a capability table (groups, link to `docs/endpoint-coverage.md`), the quality statement (100% coverage, 0 Codacy issues, with the date), error handling, retry behaviour, logging, and links (NuGet, repo, issues). Confirm the quick-start code compiles by pasting it into a scratch test (do not commit that test).

- [ ] **Step 2: Pack and inspect.** `dotnet pack TheHive.Api/TheHive.Api.csproj -c Release`; confirm `.nupkg` and `.snupkg`, and the package contains `README.md` and `Logo.png`.

- [ ] **Step 3: Governance.** Run this repo's scanner against `TheHive.Api` (see `PanoramicData.NugetManagement` docs) and fix any non-compliance.

- [ ] **Step 4: Commit** (`docs: README, contributing and publishing guide`).

---

### Task 13: Integration tests (blocked on OPS-157846)

**Files:**
- Create: `TheHive.Api.Test.Integration/` project (csproj, `xunit.runner.json` with `failSkips: false`, `usersecrets.example.json`, `Fixture.cs`, one smoke test per group)
- Modify: `TheHive.Api.slnx` (add project)

**Do not start until the user confirms the test environment is ready.**

- [ ] **Step 1: Project.** Copy the Highlight.Api.Test project structure (user-secrets, `Skip` when `TheHive:BaseUrl` or `TheHive:ApiKey` is missing, with a comment documenting why `failSkips` is false here).
- [ ] **Step 2: Smoke tests.** Create/get/update/delete a case; list via the query builder; create alert -> promote; create task + log; observable create/delete; user `GetCurrentAsync`; each test cleans up what it creates.
- [ ] **Step 3: Verify response shapes.** Any model field marked `(verify)` in the coverage doc is checked against the live response; fix models and their unit tests.
- [ ] **Step 4: Confirm the unit-coverage gate is unaffected** (the integration project is excluded from CI test and coverage), then commit (`test: integration tests against the OPS-157846 environment`).

---

## Self-Review

- **Spec coverage:** Goal/criteria -> Tasks 2, 11. Full API scope -> Tasks 3, 6-9. Structure -> Task 1 and File Structure. Components (options, handler, converters, exception, query builder) -> Tasks 4, 5, 7. Testing section -> Tasks 5-10 and Group Recipe. Repo/CI -> Tasks 1, 2, 12. Build order -> task order. Integration tests -> Task 13. Risk (spec access) -> Task 3.
- **Deviations from spec, deliberate:** (a) the spec says "reflection test asserts every Refit method is exercised"; reflection cannot prove execution, so Task 10 asserts structure and the 100% coverage gate proves exercise. (b) Refit paths are relative, not leading-slash (Review Focus 6).
- **Open items the spec cannot settle:** exact endpoint list and field sets come from the OpenAPI spec (Task 3); rows marked `(verify)` are corrected before tests are written.
- **Type consistency:** `TestClient.Create`, `StubHandler.Enqueue`, `TheHiveJson.Options`, `TheHiveApiException` members and `ICases` signatures are used identically in Tasks 5-10.
