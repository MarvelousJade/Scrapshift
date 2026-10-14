param(
    [string]$PlayerPath = 'Builds/Performance/Scrapshift.exe',
    [string]$OutputDirectory = 'artifacts/performance/current'
)
$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path -Parent $PSScriptRoot
if (-not [IO.Path]::IsPathRooted($PlayerPath)) { $PlayerPath = Join-Path $gameRoot $PlayerPath }
if (-not [IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory = Join-Path $gameRoot $OutputDirectory }
$player = (Resolve-Path -LiteralPath $PlayerPath).Path
New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$output = (Resolve-Path -LiteralPath $OutputDirectory).Path
$reportPath = Join-Path $output 'performance.json'
$logPath = Join-Path $output 'player.log'
$started = [DateTime]::UtcNow
$arguments = '-batchmode -nographics --performance-review "{0}" -logFile "{1}"' -f $output, $logPath
$process = Start-Process -FilePath $player -WorkingDirectory $gameRoot -ArgumentList $arguments -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Performance player failed: $($process.ExitCode). See $logPath" }
if (-not (Test-Path -LiteralPath $reportPath) -or (Get-Item -LiteralPath $reportPath).LastWriteTimeUtc -lt $started) {
    throw 'The benchmark did not produce a fresh report.'
}
$report = Get-Content -LiteralPath $reportPath -Raw | ConvertFrom-Json
if ($report.samples.Count -ne 2 -or $report.samples[0].enemies -ne 100 -or $report.samples[1].enemies -ne 200) {
    throw 'The report did not contain both complete enemy scenarios.'
}
foreach ($sample in $report.samples) {
    if ($sample.frames -ne 360 -or $sample.shotsFired -le 0 -or $sample.simulationMeanMs -le 0) {
        throw 'The benchmark did not complete its expected combat workload.'
    }
}
$report.samples | Select-Object enemies, frames, simulationMeanMs, simulationP95Ms, allocationCounterValid, allocatedBytesPerFrame
Write-Output "Report: $reportPath"
