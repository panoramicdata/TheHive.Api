# TheHive.Api integration tests

Smoke tests that run the client against a real TheHive 5 instance. They are not part of CI or of unit test coverage.

- Read-only: current user, permission list.
- Mutating (safe for a shared dev instance): case lifecycle (create, get, update, clear a field, delete, 404), a case with a task, observable and comment, an alert lifecycle, a custom field lifecycle, case and alert attachment uploads (multipart, including a non-seekable stream), an organization attachment used as a file observable and downloaded with and without `asZip`, a case merge, and an unshare `DELETE` with a JSON body. The read-only license challenge (text/plain) runs in the `admin` organization and is never printed. Everything created is named `[TheHive.Api integration] <guid>` (custom fields `itest<guid>`) and deleted in a `finally` block.

## Configuration

The tests read the user-secrets section `Config`. If `BaseAddress` or `ApiKey` is missing (or `BaseAddress` is still the example placeholder), every test is **skipped** with a reason; no test fails merely because secrets are absent. This project sets `"failSkips": false` in `xunit.runner.json` for that reason; it is the only project permitted to do so (the unit test project fails on any skip).

```
dotnet user-secrets set "Config:BaseAddress" "<url>" --project TheHive.Api.Test.Integration
dotnet user-secrets set "Config:ApiKey" "<api key>" --project TheHive.Api.Test.Integration
dotnet user-secrets set "Config:Organisation" "<organisation name>" --project TheHive.Api.Test.Integration
```

`Config:Organisation` is optional. See `usersecrets.example.json` for the shape. Never commit real values.

## Running

```
dotnet test --project TheHive.Api.Test.Integration
```
