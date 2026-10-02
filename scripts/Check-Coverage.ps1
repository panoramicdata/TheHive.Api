param(
	[Parameter(Mandatory)][string]$File,
	[double]$Threshold = 100,
	# Only for experiments: accepts a report that instruments nothing. CI never uses it.
	[switch]$AllowEmpty
)

[xml]$xml = Get-Content -Raw $File
$coverage = $xml.coverage
if ($null -eq $coverage) {
	Write-Error "'$File' is not a cobertura coverage report."
	exit 1
}

$linesValid = if ($coverage.'lines-valid') { [int]$coverage.'lines-valid' } else { 0 }
$branchesValid = if ($coverage.'branches-valid') { [int]$coverage.'branches-valid' } else { 0 }
$packages = @($coverage.packages.package).Where({ $null -ne $_ }).Count
Write-Host ("Packages: {0}, coverable lines: {1}, coverable branches: {2}" -f $packages, $linesValid, $branchesValid)

if (-not $AllowEmpty -and ($packages -eq 0 -or $linesValid -le 0)) {
	Write-Error "The coverage report is empty (no packages or no coverable lines): nothing was instrumented, so 100% would be meaningless."
	exit 1
}

$line = [double]$coverage.'line-rate' * 100
# The branch rate is only meaningful when there are branches to cover.
$branch = if ($branchesValid -gt 0) { [double]$coverage.'branch-rate' * 100 } else { 100.0 }
Write-Host ("Line coverage:   {0:N2}%" -f $line)
Write-Host ("Branch coverage: {0:N2}%" -f $branch)
if ($line -lt $Threshold -or $branch -lt $Threshold) {
	Write-Error "Coverage is below the required $Threshold%."
	exit 1
}
