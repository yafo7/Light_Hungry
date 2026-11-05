param([switch]$Validate)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject=Join-Path $taskWorkspace 'UnityProject'
$taskValidation=Join-Path $taskWorkspace 'Validation/level02'
[IO.Directory]::CreateDirectory($taskValidation) | Out-Null
$taskOpen=Get-CimInstance Win32_Process | Where-Object {$_.Name -eq 'Unity.exe' -and $_.CommandLine -like ('*'+$taskProject+'*')}
if($taskOpen){
  # A fresh editor process prevents an unfocused editor from running an old loaded builder assembly.
  $taskCheckout=Join-Path $taskWorkspace '.build-level02/UnityProject'
  [IO.Directory]::CreateDirectory($taskCheckout) | Out-Null
  foreach($taskFolder in @('Assets','ProjectSettings','Packages')){Copy-Item -LiteralPath (Join-Path $taskProject $taskFolder) -Destination $taskCheckout -Recurse -Force}
  $taskPreviousWorkspace=$env:LIGHTFEAST_WORKSPACE
  try {
    $env:LIGHTFEAST_WORKSPACE=$taskWorkspace
    $taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskCheckout+'"'),'-executeMethod','SecondLevelBuilder.BuildAll','-logFile',('"'+(Join-Path $taskValidation 'isolated-build.log')+'"')) -WindowStyle Hidden -PassThru
    $taskProcess.WaitForExit()
    if($taskProcess.ExitCode -ne 0){throw 'Isolated Unity build failed; see Validation/level02/isolated-build.log'}
  } finally {if($null -eq $taskPreviousWorkspace){Remove-Item Env:LIGHTFEAST_WORKSPACE -ErrorAction SilentlyContinue}else{$env:LIGHTFEAST_WORKSPACE=$taskPreviousWorkspace}}
  foreach($taskDir in @('Assets/Art/Level02','Assets/Art/Level02Geometry','Assets/Materials/Level02')){Copy-Item -LiteralPath (Join-Path $taskCheckout $taskDir) -Destination (Join-Path $taskProject (Split-Path $taskDir -Parent)) -Recurse -Force}
  foreach($taskScene in @('FirstLight','SecondLight','ThirdDrain')){foreach($taskExt in @('.unity','.unity.meta')){Copy-Item -LiteralPath (Join-Path $taskCheckout ('Assets/Scenes/'+$taskScene+$taskExt)) -Destination (Join-Path $taskProject ('Assets/Scenes/'+$taskScene+$taskExt)) -Force}}
  Copy-Item -LiteralPath (Join-Path $taskCheckout 'ProjectSettings/EditorBuildSettings.asset') -Destination (Join-Path $taskProject 'ProjectSettings/EditorBuildSettings.asset') -Force
} else {
  $taskUnity='C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe'
  $taskProcess=Start-Process -FilePath $taskUnity -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod','SecondLevelBuilder.BuildAll','-logFile',('"'+(Join-Path $taskValidation 'build.log')+'"')) -WindowStyle Hidden -PassThru
  $taskProcess.WaitForExit()
  if($taskProcess.ExitCode -ne 0){throw 'Unity build failed; see Validation/level02/build.log'}
}
if($Validate){
 foreach($taskTest in @(@{Flag='--validate-level02';Dir='native-run';Report='playthrough.json'},@{Flag='--validate-level-link';Dir='level-link';Report='first-to-second.json'})){
  $taskOut=Join-Path $taskValidation $taskTest.Dir
  [IO.Directory]::CreateDirectory($taskOut) | Out-Null
  $taskPlayer=Start-Process -FilePath (Join-Path $taskWorkspace 'Builds/Windows-Level02/LightFeast.exe') -ArgumentList @($taskTest.Flag,'-validationOutput',('"'+$taskOut+'"'),'-logFile',('"'+(Join-Path $taskOut 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810') -WindowStyle Hidden -PassThru
  $taskPlayer.WaitForExit()
  Get-Content -LiteralPath (Join-Path $taskOut $taskTest.Report)
  if($taskPlayer.ExitCode -ne 0){throw ('Native test failed: '+$taskTest.Flag)}
 }
}
