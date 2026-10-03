# Publishing TheHive.Api to NuGet

> **Nothing has been published.** No `TheHive.Api` package exists on NuGet.org yet, no release tag has been pushed, and trusted publishing has not been configured. Publishing needs the repository owner's explicit go-ahead; do not run `Publish.ps1` or push a version tag without it.

Releases are cut by pushing a version tag. The CI workflow (`.github/workflows/ci.yml`) builds, tests (with the 100% coverage gate), packs, and, **only for a tag**, publishes to NuGet.org using **trusted publishing** (GitHub OIDC through `NuGet/login`). No long-lived NuGet API key is stored in the repository or in GitHub secrets.

## Versioning

[Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) (NBGV) computes the version from `version.json` (currently `"version": "5.8"`) and the Git height, so a release built from `main` is `5.8.<height>`; other branches get a `-g<hash>` suffix. `version.json` marks `main` and tags of the form `N.N.N` as public releases (no `-g<hash>` suffix). NBGV is referenced by the project (`Nerdbank.GitVersioning` package), so no `nbgv` global tool is needed.

Major.minor is the TheHive API version the client targets (`5.8` = spec v5.8.0), not a SemVer signal for this client: a breaking change to the client before the next TheHive version ships as a patch bump. Change `version.json` only when the client moves to a new TheHive version (for example to `5.9`), in the same change that replaces the vendored spec (`docs/openapi/thehive-docs.yaml`) and regenerates `docs/endpoint-coverage.md` (see "Updating for a new spec version" in [CONTRIBUTING.md](CONTRIBUTING.md)).

## First-time setup (once, by the owner)

1. Create the package ID on NuGet.org. Trusted publishing can be configured for a package ID before it exists, as a pending trusted publisher: sign in at <https://www.nuget.org>, open **Trusted Publishing** in your account menu, and add a policy with
   - Repository owner: `panoramicdata`
   - Repository: `TheHive.Api`
   - Workflow file: `ci.yml`
   - Environment: leave empty (the workflow does not use a GitHub environment)
2. Confirm the `user` input of the `NuGet/login@v1` step in `ci.yml` is the NuGet.org **profile name** that owns the policy (it is currently set to the owner's profile name).
3. Make sure the repository's `publish` job keeps `permissions: id-token: write`; the OIDC token is what NuGet.org exchanges for a short-lived key.
4. Optionally enable private vulnerability reporting on the GitHub repository (see `SECURITY.md`).

## Releasing

Run from a clean checkout of `main` that is level with `origin/main`:

```powershell
.\Publish.ps1
```

The script stops with an error unless every condition holds:

1. The working tree is clean (`git status --porcelain` is empty).
2. The current branch is `main`.
3. Local `HEAD` equals `origin/main` (after a `git fetch`).
4. NBGV can compute a version (it builds `TheHive.Api/TheHive.Api.csproj` with `-t:GetBuildVersion`).
5. The tag does not already exist.

It then creates the tag (for example `5.8.7`, no `v` prefix) on the current commit and pushes it. That tag triggers CI (`tags: ['[0-9]*.[0-9]*.[0-9]*']`), whose `publish` job downloads the `.nupkg` and `.snupkg` built by the `build` job, logs in through `NuGet/login`, and runs `dotnet nuget push ... --skip-duplicate`. Watch the run on the repository's Actions tab. The package normally appears on NuGet.org within minutes, and symbols are published with it.

Before tagging, run the checks locally:

```powershell
dotnet build TheHive.Api/TheHive.Api.csproj -c Release
dotnet test --project TheHive.Api.Test -c Release --coverage --coverage-output-format cobertura --coverage-output coverage.cobertura.xml --coverage-settings coverage.settings.xml
./scripts/Check-Coverage.ps1 -File (Get-ChildItem -Recurse -Filter coverage.cobertura.xml | Select-Object -First 1).FullName
dotnet pack TheHive.Api/TheHive.Api.csproj -c Release --output ./artifacts
```

Inspect the `.nupkg` (it is a zip): it must contain `README.md`, `Logo.png`, `lib/net10.0/TheHive.Api.dll` and `lib/net10.0/TheHive.Api.xml`.

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| `Working tree is not clean` | Commit or stash. Untracked files count. |
| `Publishing is only supported from the 'main' branch` | Merge to `main` through a pull request, then check out `main`. |
| `Local branch is not up to date with origin/main` | `git pull` (or push your commits) and rerun. |
| `Tag '5.8.N' already exists` | Make a new commit so the Git height advances. Bump `version.json` only when moving to a new TheHive version. |
| `Failed to determine version` | Restore and build the project once (`dotnet build TheHive.Api/TheHive.Api.csproj`); make sure `version.json` is valid JSON and the repository is not a shallow clone. |
| The tag was pushed but no CI run started | The tag must look like `5.8.7`; a `v5.8.7` tag does not match the trigger. |
| `publish` job fails at NuGet login (401/403) | The trusted publishing policy is missing or does not match the owner, repository, workflow file name (`ci.yml`) or the `user` profile name; or `id-token: write` was removed. |
| `409 Conflict` / version already exists | `--skip-duplicate` makes the push a no-op; bump the version and tag again. |
| Run succeeded but the package is not listed | Indexing can lag by several minutes; check the package's **Manage** page for validation status. |
| A CI step fails on coverage | The gate requires 100.00% line and branch coverage; add the missing tests (never exclude code to pass). |

To retag after a failed release you must delete the tag locally and on the remote; that rewrites published history, so only do it with the owner's approval.
