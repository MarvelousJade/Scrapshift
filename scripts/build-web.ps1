param(
    [string]$UnityPath = 'C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.com',
    [string]$ProjectPath = (Split-Path -Parent $PSScriptRoot),
    [switch]$Review
)
$ErrorActionPreference='Stop'
New-Item -ItemType Directory -Force (Join-Path $ProjectPath 'artifacts') | Out-Null
$buildArgs=@('-batchmode','-nographics','-quit','-projectPath',$ProjectPath,'-buildTarget','WebGL','-executeMethod','Scrapshift.Editor.WebBuild.Build','-logFile',(Join-Path $ProjectPath 'artifacts/web-build.log'))
if($Review){$buildArgs+='--web-review'}
& $UnityPath @buildArgs
if($LASTEXITCODE -ne 0){throw 'Web build failed; see artifacts/web-build.log'}
Get-Content (Join-Path $ProjectPath 'artifacts/web-build.txt')
