# Contributing

Thank you for your interest in contributing to TheHive.Api!

## How to Contribute

1. **Fork** the repository (or branch from `main` if you have write access)
2. **Create a branch** for your feature or fix (`git checkout -b feature/my-feature`)
3. **Make your changes** following the coding standards below
4. **Write or update tests** as appropriate
5. **Ensure the build passes** with zero errors, zero warnings, and zero messages
6. **Run the unit tests** and the coverage check (see Testing)
7. **Submit a Pull Request** against the `main` branch with a clear description of what changed and why

Use short, imperative commit messages in the style `feat: ...`, `fix: ...`, `docs: ...`, `test: ...`, `chore: ...`. Do not commit directly to `main`, and do not force-push or rewrite published history. Only the repository owner publishes releases (see [PUBLISHING.md](PUBLISHING.md)).

Follow `.editorconfig`: tabs for C#, XML, MSBuild and JSON, spaces for YAML, and do not reformat code you are not changing. Fix any `IDE0055` formatting diagnostics in the files you touch.

## Coding Standards

- All public members must have XML documentation comments
- Use `System.Text.Json` - do not introduce `Newtonsoft.Json`
- Use Refit for HTTP client interfaces
- Use file-scoped namespaces
- Use the `required` keyword for DTO properties where appropriate
- Ensure `TreatWarningsAsErrors` remains enabled; never add `NoWarn` or suppress a diagnostic to get a build through
- All code must compile with zero diagnostics
- Use central package management: versions live in `Directory.Packages.props`, never in a `.csproj`

### Models and naming

- **One type per spec schema**, one type per file, in `TheHive.Api/Data/{Group}/` (namespace `TheHive.Api.Data.{Group}`). Cover every property of the schema with `[JsonPropertyName]` and an XML comment.
- Output entities take the entity name (`Case`); an endpoint's top-level request bodies are `{Entity}CreateRequest` and `{Entity}UpdateRequest`. A schema nested inside a body is named for what it is (for example `ShareSettings`).
- **`Optional<T>`** (in `Data/Common/`) is used for every update field the spec says can be cleared by sending `null`. `default` means "leave unchanged"; a set `null` clears the field. Plain nullable properties are omitted when null.
- Epoch-millisecond integers are `DateTimeOffset`; string enums are tolerant enums with `Unknown = 0` first; shared value types live in `Data/Common/` and are reused, not duplicated.
- Interfaces are `TheHive.Api/Interfaces/I{Group}.cs`, with Refit paths that are relative (no leading slash), parameter names matching the spec, and a trailing `CancellationToken` on every method. Each interface is exposed as a property on `TheHiveClient`.
- Never log or print the API key, query strings or secret-bearing models; document every secret-bearing property as `SECRET`.

The long-form conventions (uploads, downloads, DELETE with a body, connectors and secrets, querying) are in the "Implementation notes" of [docs/endpoint-coverage.md](docs/endpoint-coverage.md). Read them before adding an operation.

## Endpoint coverage

`docs/endpoint-coverage.md` is the **source of truth** for what the client implements, and `InventoryTests` enforces it:

- Every spec operation is one row: group, verb, path, client method (`` `IGroup.MethodAsync` ``) and test (`` `Class.TestMethod` ``). Several methods or tests may be listed, comma-separated, each in backticks. Fill both cells or neither.
- Every method of every interface in `TheHive.Api.Interfaces` must be listed, every reference must resolve by reflection, and every non-deprecated row must be filled. Deprecated operations are marked `(deprecated)` in the Path column and are not implemented.
- Put notes such as `(verify: ...)` after the path, in the Path column, for operations whose spec is untyped or example-derived and not yet confirmed against a live server. Keep the path itself in backticks and do not use a pipe (`|`) in a note.
- When you add or rename an operation or test, update the table in the same commit, and update the "Coverage status" counts.

### Updating for a new spec version

1. Download the new spec to `docs/openapi/thehive-docs.yaml` (<https://docs.strangebee.com/thehive/api-docs/docs.yaml>) and note its `info.version`.
2. Diff it against the old file (`git diff`) to find new, changed, removed and newly deprecated operations.
3. Add new rows to the table (and to the "Summary by spec tag" table); mark newly deprecated operations `(deprecated)`; remove rows for operations the spec dropped (and the corresponding client methods).
4. Implement the new operations following the Cases group as the worked template, with tests, until `InventoryTests` is green.
5. Update the spec version in the document header and in the README, and verify against a live server where possible.
6. Bump `version.json` to the new TheHive major.minor (for example `"5.9"`) in the same change. The package version follows the targeted TheHive version (see "Versioning" in the README and [PUBLISHING.md](PUBLISHING.md)); do not bump it for any other reason.

## Testing

- Use xunit v3 (Microsoft Testing Platform runner) for all tests
- Use AwesomeAssertions for fluent assertions
- Do not skip, delete or weaken tests to make a build pass; the unit test project fails on any skip
- Every method needs a test asserting verb, path and body, and a full JSON sample mapped into the model
- Keep **100% line and branch coverage**:

```powershell
dotnet test --project TheHive.Api.Test -c Release --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --coverage-settings coverage.settings.xml
./scripts/Check-Coverage.ps1 -File (Get-ChildItem -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1).FullName
```

- Build only the project you changed when verifying (for example `dotnet build TheHive.Api/TheHive.Api.csproj`).
- Live integration tests (`TheHive.Api.Test.Integration`) run against a real TheHive 5 instance, are skipped without user-secrets, and are not part of CI. They are described in [TheHive.Api.Test.Integration/README.md](TheHive.Api.Test.Integration/README.md).

### Secrets handling

- Never commit API keys, host names, organisation names or any other credential, in source, tests, docs, logs or screenshots. Test data must use obviously fake values (`example.com`, `analyst@example.com`).
- Store integration settings only in your own user-secrets (`dotnet user-secrets set ... --project TheHive.Api.Test.Integration`), never in files in the repository. Do not paste the output of `dotnet user-secrets list` anywhere.
- If you think you committed a secret, rotate it immediately and tell the maintainers privately (see [SECURITY.md](SECURITY.md)).

## License

By contributing, you agree that your contributions will be licensed under the MIT License.
