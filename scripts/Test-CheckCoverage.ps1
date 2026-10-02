# Self-test for Check-Coverage.ps1: runs it against synthetic cobertura reports and asserts the exit code of each.
$ErrorActionPreference = 'Stop'
$checker = Join-Path $PSScriptRoot 'Check-Coverage.ps1'
$dir = Join-Path ([IO.Path]::GetTempPath()) ("check-coverage-test-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $dir | Out-Null
$failures = 0

function Invoke-Case([string]$name, [string]$xml, [int]$expected) {
	$path = Join-Path $dir "$name.xml"
	Set-Content -Path $path -Value $xml
	pwsh -NoProfile -File $checker -File $path *> $null
	$actual = $LASTEXITCODE
	if ($actual -eq $expected) {
		Write-Host "PASS $name (exit $actual)"
	} else {
		Write-Host "FAIL ${name}: expected exit $expected, got $actual"
		$script:failures++
	}
}

$pkg = '<packages><package name="p"><classes /></package></packages>'
Invoke-Case 'populated-100' "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" 0
Invoke-Case 'populated-99-line' "<coverage line-rate=`"0.99`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" 1
Invoke-Case 'populated-50-branch' "<coverage line-rate=`"1`" branch-rate=`"0.5`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" 1
Invoke-Case 'no-branches-100-line' "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"0`">$pkg</coverage>" 0
Invoke-Case 'empty-report' '<coverage line-rate="1" branch-rate="1"><packages /></coverage>' 1
Invoke-Case 'empty-with-zero-lines' "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"0`" branches-valid=`"0`">$pkg</coverage>" 1
Invoke-Case 'missing-attributes' "<coverage>$pkg</coverage>" 1
Invoke-Case 'no-packages' '<coverage line-rate="1" branch-rate="1" lines-valid="100" branches-valid="10"><packages /></coverage>' 1
Invoke-Case 'not-cobertura' '<other />' 1

Remove-Item -Recurse -Force $dir
if ($failures -gt 0) {
	Write-Error "$failures self-test case(s) failed."
	exit 1
}
Write-Host 'All Check-Coverage self-tests passed.'
