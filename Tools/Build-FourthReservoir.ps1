param([switch]$Validate,[switch]$PlayerOnly,[switch]$ShowPlayer)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject=Join-Path $taskWorkspace 'UnityProject-NOVA-URP'
$taskValidation=Join-Path $taskWorkspace 'Validation/level04'
[IO.Directory]::CreateDirectory($taskValidation) | Out-Null
$taskOpen=Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {$_.CommandLine -like ('*'+$taskProject+'*')}
if($taskOpen){throw 'The NOVA Unity project is open. Save and close it before running this batch build.'}
$taskMethod=if($PlayerOnly){'FourthLevelBuilder.BuildPlayer'}else{'FourthLevelBuilder.BuildAll'}
$taskPrevious=$env:LIGHTFEAST_WORKSPACE
try {
 $env:LIGHTFEAST_WORKSPACE=$taskWorkspace
 $taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod',$taskMethod,'-logFile',('"'+(Join-Path $taskValidation 'build.log')+'"')) -WindowStyle Hidden -PassThru
 $taskProcess.WaitForExit()
 if($taskProcess.ExitCode -ne 0){throw 'Level04 build failed; see Validation/level04/build.log'}
} finally {if($null -eq $taskPrevious){Remove-Item Env:LIGHTFEAST_WORKSPACE -ErrorAction SilentlyContinue}else{$env:LIGHTFEAST_WORKSPACE=$taskPrevious}}
Get-Content -LiteralPath (Join-Path $taskValidation 'build-result.json')
if($Validate){
 $taskOut=Join-Path $taskValidation 'native-run'
 [IO.Directory]::CreateDirectory($taskOut) | Out-Null
 $taskWindowStyle=if($ShowPlayer){'Normal'}else{'Hidden'}
 $taskPlayer=Start-Process -FilePath (Join-Path $taskWorkspace 'Builds/Windows-Level04-NOVA/LightFeast.exe') -ArgumentList @('--validate-level04','-validationOutput',('"'+$taskOut+'"'),'-logFile',('"'+(Join-Path $taskOut 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810') -WindowStyle $taskWindowStyle -PassThru
 $taskPlayer.WaitForExit()
 Get-Content -LiteralPath (Join-Path $taskOut 'playthrough.json') -Raw | ConvertFrom-Json | Select-Object completed,seconds,jumps,deaths,transformations
 if($taskPlayer.ExitCode -ne 0){throw 'Level04 input validation failed; see Validation/level04/native-run/playthrough.json'}
}
