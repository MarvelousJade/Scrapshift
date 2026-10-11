param([string]$UnityPath = 'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Unity.com')
$ErrorActionPreference = 'Stop'
$gameRoot = Split-Path -Parent $PSScriptRoot
New-Item -ItemType Directory -Path (Join-Path $gameRoot 'artifacts/raw') -Force | Out-Null
& $UnityPath -batchmode -nographics -quit -projectPath $gameRoot -executeMethod Scrapshift.Editor.ContentBuilder.Build -logFile "$gameRoot/artifacts/raw/build.log"
if ($LASTEXITCODE -ne 0) { throw "Unity build failed: $LASTEXITCODE" }
Get-Content -LiteralPath "$gameRoot/artifacts/build.txt"
