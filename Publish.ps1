# Panoramic Data NuGet Publish Script (Standard)
# Tags the current commit with the NBGV version and pushes to trigger CI/CD publishing.
# Usage: .\Publish.ps1

$ErrorActionPreference = 'Stop'

# Status messages go through Write-Information so they can be captured or redirected.
# $InformationPreference is set here so the messages still show by default when run interactively.
$InformationPreference = 'Continue'

function Write-Status {
    param(
        [Parameter(Position = 0)]
        [AllowEmptyString()]
        [string]$Message = '',

        [ValidateSet('Default', 'Red', 'Yellow', 'Cyan', 'Green')]
        [string]$Colour = 'Default'
    )

    # $PSStyle exists on PowerShell 7.2+ and strips the escape sequences itself when the
    # output is redirected. On older hosts the lookups yield $null and the text is uncoloured.
    $style = switch ($Colour) {
        'Red' { $PSStyle.Foreground.BrightRed }
        'Yellow' { $PSStyle.Foreground.BrightYellow }
        'Cyan' { $PSStyle.Foreground.BrightCyan }
        'Green' { $PSStyle.Foreground.BrightGreen }
        default { $null }
    }

    if ($style) {
        Write-Information "$style$Message$($PSStyle.Reset)"
    } else {
        Write-Information $Message
    }
}

# Check for clean working tree (porcelain)
$status = git status --porcelain
if ($status) {
    Write-Error "Working tree is not clean. Commit or stash changes before publishing.`n$status"
    exit 1
}

# Ensure we are on the main branch
$branch = git rev-parse --abbrev-ref HEAD
if ($branch -ne 'main') {
    Write-Error "Publishing is only supported from the 'main' branch (currently on '$branch')."
    exit 1
}

# Ensure local main is up to date with remote
git fetch origin main --quiet
$localHead = git rev-parse HEAD
$remoteHead = git rev-parse origin/main
if ($localHead -ne $remoteHead) {
    Write-Error "Local branch is not up to date with origin/main. Pull or push first."
    exit 1
}

# Get version from NBGV
# Get version from Nerdbank.GitVersioning via the project's MSBuild targets (the
# referenced NuGet package), so this does not depend on the global 'nbgv' CLI tool
# being installed or on PATH. The GetBuildVersion target must run for the computed
# version to be populated (a plain -getProperty evaluation returns the static
# version.json value without the Git height).
$project = Join-Path $PSScriptRoot 'TheHive.Api/TheHive.Api.csproj'
$buildOutput = dotnet build $project -t:GetBuildVersion --getProperty:NuGetPackageVersion -nologo -v:quiet -p:TreatWarningsAsErrors=false
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to determine version from Nerdbank.GitVersioning.`n$buildOutput"
    exit 1
}
$version = ($buildOutput | Select-Object -Last 1).ToString().Trim()

if (-not $version) {
    Write-Error "Failed to determine version from nbgv."
    exit 1
}

# Check tag doesn't already exist
$existingTag = git tag -l $version
if ($existingTag) {
    Write-Error "Tag '$version' already exists."
    exit 1
}

Write-Status "Tagging as $version ..." -Colour Cyan
git tag $version
git push origin $version

Write-Status "✅ Published tag $version — CI will build and push to NuGet." -Colour Green
