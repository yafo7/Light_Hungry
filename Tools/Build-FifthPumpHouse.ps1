param([switch]$PlayerOnly,[switch]$Validate,[switch]$ShowPlayer)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject=Join-Path $taskWorkspace 'UnityProject-NOVA-URP'
$taskOutput=Join-Path $taskWorkspace 'Validation/level05'
[IO.Directory]::CreateDirectory($taskOutput) | Out-Null
$taskOpen=Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {$_.CommandLine -like ('*'+$taskProject+'*')}
if($taskOpen){throw 'Save and close this Unity project before batch building.'}
$taskMethod=if($PlayerOnly){'FifthLevelBuilder.BuildPlayer'}else{'FifthLevelBuilder.BuildAll'}
$taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod',$taskMethod,'-logFile',('"'+(Join-Path $taskOutput 'build.log')+'"')) -WindowStyle Hidden -PassThru
$taskProcess.WaitForExit()
if($taskProcess.ExitCode -ne 0){throw 'Fifth level build failed. See Validation/level05/build.log'}
Get-Content -LiteralPath (Join-Path $taskOutput 'build-result.json')
if($Validate){
 $taskStyle=if($ShowPlayer){'Normal'}else{'Hidden'}
 $taskPlayer=Start-Process -FilePath (Join-Path $taskWorkspace 'Builds/Windows-Level05-NOVA/LightFeast.exe') -ArgumentList @('--validate-level05','-logFile',('"'+(Join-Path $taskOutput 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810') -WindowStyle $taskStyle -PassThru
 $taskPlayer.WaitForExit()
 Get-Content -LiteralPath (Join-Path $taskOutput 'native-run/playthrough.json')
 if($taskPlayer.ExitCode -ne 0){throw 'Fifth level input validation failed.'}
}
