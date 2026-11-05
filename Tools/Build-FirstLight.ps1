param([switch]$Validate)
$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $workspace 'UnityProject'
$editor = 'C:\Program Files\Unity\Hub\Editor\2021.3.42f1c1\Editor\Unity.exe'
if (-not (Test-Path -LiteralPath $editor)) { throw 'Please install Unity 2021.3.42f1c1 or update the editor path in this script.' }
$log = Join-Path $workspace 'Validation\build.log'
$process = Start-Process -FilePath $editor -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$project+'"'),'-executeMethod','FirstLevelBuilder.BuildAll','-logFile',('"'+$log+'"')) -PassThru -WindowStyle Hidden
$process.WaitForExit()
if ($process.ExitCode -ne 0) { throw "Unity build failed. See $log" }
if ($Validate) {
    $output = Join-Path $workspace 'Validation\native-run'
    [IO.Directory]::CreateDirectory($output) | Out-Null
    $player = Join-Path $workspace 'Builds\Windows\LightFeast.exe'
    $playerLog = Join-Path $output 'Player.log'
    # Native rendering needs a visible game window; hidden Windows players can capture black frames.
    $run = Start-Process -FilePath $player -ArgumentList @('--validate','-validationOutput',('"'+$output+'"'),'-logFile',('"'+$playerLog+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810') -PassThru
    $run.WaitForExit()
    Get-Content -LiteralPath (Join-Path $output 'playthrough.json')
}
