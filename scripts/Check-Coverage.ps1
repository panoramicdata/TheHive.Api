param(
	[Parameter(Mandatory)][string]$File,
	[double]$Threshold = 100
)

[xml]$xml = Get-Content -Raw $File
$linesValid = if ($xml.coverage.'lines-valid') { [int]$xml.coverage.'lines-valid' } else { -1 }
$branchesValid = if ($xml.coverage.'branches-valid') { [int]$xml.coverage.'branches-valid' } else { -1 }
# No coverable lines/branches (e.g. only marker types) counts as fully covered.
if ($linesValid -eq 0 -and $branchesValid -eq 0) {
	$line = 100.0
	$branch = 100.0
} else {
	$line = [double]$xml.coverage.'line-rate' * 100
	$branch = [double]$xml.coverage.'branch-rate' * 100
}
Write-Host ("Line coverage:   {0:N2}%" -f $line)
Write-Host ("Branch coverage: {0:N2}%" -f $branch)
if ($line -lt $Threshold -or $branch -lt $Threshold) {
	Write-Error "Coverage is below the required $Threshold%."
	exit 1
}
