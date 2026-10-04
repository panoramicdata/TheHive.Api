# Self-test for Check-Coverage.ps1: runs it against synthetic cobertura reports and asserts the exit code of each.
$ErrorActionPreference = 'Stop'

# Status messages go through Write-Information so they can be captured or redirected.
# $InformationPreference is set here so the messages still show by default (including in CI logs).
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

$checker = Join-Path $PSScriptRoot 'Check-Coverage.ps1'
$dir = Join-Path ([IO.Path]::GetTempPath()) ("check-coverage-test-" + [guid]::NewGuid())
New-Item -ItemType Directory -Path $dir | Out-Null
$failures = 0

function Invoke-Case {
	param(
		[string]$Name,
		[string]$Xml,
		[int]$Expected
	)

	$path = Join-Path $dir "$name.xml"
	Set-Content -Path $path -Value $xml
	pwsh -NoProfile -File $checker -File $path *> $null
	$actual = $LASTEXITCODE
	if ($actual -eq $expected) {
		Write-Status "PASS $name (exit $actual)"
	} else {
		Write-Status "FAIL ${name}: expected exit $expected, got $actual"
		$script:failures++
	}
}

$pkg = '<packages><package name="p"><classes /></package></packages>'
Invoke-Case -Name 'populated-100' -Xml "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" -Expected 0
Invoke-Case -Name 'populated-99-line' -Xml "<coverage line-rate=`"0.99`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" -Expected 1
Invoke-Case -Name 'populated-50-branch' -Xml "<coverage line-rate=`"1`" branch-rate=`"0.5`" lines-valid=`"100`" branches-valid=`"20`">$pkg</coverage>" -Expected 1
Invoke-Case -Name 'no-branches-100-line' -Xml "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"100`" branches-valid=`"0`">$pkg</coverage>" -Expected 0
Invoke-Case -Name 'empty-report' -Xml '<coverage line-rate="1" branch-rate="1"><packages /></coverage>' -Expected 1
Invoke-Case -Name 'empty-with-zero-lines' -Xml "<coverage line-rate=`"1`" branch-rate=`"1`" lines-valid=`"0`" branches-valid=`"0`">$pkg</coverage>" -Expected 1
Invoke-Case -Name 'missing-attributes' -Xml "<coverage>$pkg</coverage>" -Expected 1
Invoke-Case -Name 'no-packages' -Xml '<coverage line-rate="1" branch-rate="1" lines-valid="100" branches-valid="10"><packages /></coverage>' -Expected 1
Invoke-Case -Name 'not-cobertura' -Xml '<other />' -Expected 1

Remove-Item -Recurse -Force $dir
if ($failures -gt 0) {
	Write-Error "$failures self-test case(s) failed."
	exit 1
}
Write-Status 'All Check-Coverage self-tests passed.'
exit 0
