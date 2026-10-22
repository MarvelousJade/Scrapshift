param([Parameter(Mandatory=$true)][string]$SpecFile)
$ErrorActionPreference='Stop'
$spec=Get-Content -LiteralPath $SpecFile -Raw | ConvertFrom-Json
$outputRoot=[IO.Path]::GetFullPath($spec.output)
[IO.Directory]::CreateDirectory($outputRoot) | Out-Null
$statusPath=Join-Path $outputRoot 'process.json'
$started=[DateTime]::UtcNow
$process=$null;$stdout=$null;$stderr=$null
try {
    $info=[Diagnostics.ProcessStartInfo]::new()
    $info.FileName=$spec.executable
    $info.WorkingDirectory=$spec.workingDirectory
    $info.UseShellExecute=$false
    $info.WindowStyle=[Diagnostics.ProcessWindowStyle]::Normal
    $info.RedirectStandardOutput=$true;$info.RedirectStandardError=$true
    foreach($arg in $spec.arguments){$info.ArgumentList.Add([string]$arg)}
    if($spec.environment){foreach($property in $spec.environment.psobject.Properties){$info.Environment[$property.Name]=[string]$property.Value}}
    $process=[Diagnostics.Process]::new();$process.StartInfo=$info
    if(!$process.Start()){throw 'Native fixture did not start.'}
    $stdout=[IO.File]::Open((Join-Path $outputRoot 'runner-stdout.log'),[IO.FileMode]::Create,[IO.FileAccess]::Write,[IO.FileShare]::ReadWrite)
    $stderr=[IO.File]::Open((Join-Path $outputRoot 'runner-stderr.log'),[IO.FileMode]::Create,[IO.FileAccess]::Write,[IO.FileShare]::ReadWrite)
    $stdoutTask=$process.StandardOutput.BaseStream.CopyToAsync($stdout)
    $stderrTask=$process.StandardError.BaseStream.CopyToAsync($stderr)
    @{status='running';processId=$process.Id;runnerId=$PID;startedUtc=$started.ToString('o');specFile=$SpecFile} | ConvertTo-Json | Set-Content -LiteralPath $statusPath
    $timedOut=$false
    while(!$process.WaitForExit(1000)){
        if(([DateTime]::UtcNow-$started).TotalSeconds -gt 1800){$timedOut=$true;$process.Kill();break}
    }
    while(!$process.WaitForExit(1000)){}
    [Threading.Tasks.Task]::WaitAll(@($stdoutTask,$stderrTask))
    @{status=$(if($timedOut){'timed_out'}else{'exited'});processId=$process.Id;runnerId=$PID;exitCode=$process.ExitCode;startedUtc=$started.ToString('o');finishedUtc=[DateTime]::UtcNow.ToString('o');specFile=$SpecFile} | ConvertTo-Json | Set-Content -LiteralPath $statusPath
} catch {
    @{status='runner_error';runnerId=$PID;error=$_.ToString();startedUtc=$started.ToString('o')} | ConvertTo-Json | Set-Content -LiteralPath $statusPath
    exit 1
} finally {
    if($stdout){$stdout.Dispose()};if($stderr){$stderr.Dispose()};if($process){$process.Dispose()}
}
