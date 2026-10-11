param([string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.com')
$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Path (Join-Path $gameRoot 'artifacts/raw') -Force | Out-Null
& $UnityPath -batchmode -nographics -projectPath $gameRoot -runTests -testPlatform EditMode -testFilter Scrapshift.Tests -testResults "$gameRoot/artifacts/editmode.xml" -logFile "$gameRoot/artifacts/raw/tests.log"
if ($LASTEXITCODE -ne 0) { throw "Unity tests failed: $LASTEXITCODE" }
[xml]$results = Get-Content -LiteralPath "$gameRoot/artifacts/editmode.xml"
if ($results.'test-run'.result -ne 'Passed') { throw 'Test report did not pass.' }
$results.'test-run' | Select-Object result,total,passed,failed,skipped,duration
