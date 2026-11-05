param([switch]$Validate,[switch]$CaptureWaterMotion,[switch]$CaptureRig)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject=Join-Path $taskWorkspace 'UnityProject'
$taskValidation=Join-Path $taskWorkspace 'Validation/level03'
[IO.Directory]::CreateDirectory($taskValidation) | Out-Null
$taskOpen=Get-CimInstance Win32_Process -Filter "Name = 'Unity.exe'" | Where-Object {$_.CommandLine -like ('*'+$taskProject+'*')}
$taskBuildProject=$taskProject
if($taskOpen){
 $taskBuildProject=Join-Path $taskWorkspace '.build-level03/UnityProject'
 [IO.Directory]::CreateDirectory($taskBuildProject) | Out-Null
 foreach($taskDir in @('Assets','ProjectSettings','Packages')){Copy-Item -LiteralPath (Join-Path $taskProject $taskDir) -Destination $taskBuildProject -Recurse -Force}
}
$taskPrevious=$env:LIGHTFEAST_WORKSPACE
try {
 $env:LIGHTFEAST_WORKSPACE=$taskWorkspace
 $taskProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskBuildProject+'"'),'-executeMethod','ThirdLevelBuilder.BuildAll','-logFile',('"'+(Join-Path $taskValidation 'build.log')+'"')) -WindowStyle Hidden -PassThru
 $taskProcess.WaitForExit()
 if($taskProcess.ExitCode -ne 0){throw 'ThirdDrain build failed; see Validation/level03/build.log'}
} finally {if($null -eq $taskPrevious){Remove-Item Env:LIGHTFEAST_WORKSPACE -ErrorAction SilentlyContinue}else{$env:LIGHTFEAST_WORKSPACE=$taskPrevious}}
if($taskOpen){
 foreach($taskDir in @('Assets/Art/Level03','Assets/Art/Level03Geometry','Assets/Materials/Level03','Assets/Models/Rigged','Assets/Resources')){Copy-Item -LiteralPath (Join-Path $taskBuildProject $taskDir) -Destination (Join-Path $taskProject (Split-Path $taskDir -Parent)) -Recurse -Force}
 foreach($taskFile in @('Assets/Scenes/ThirdDrain.unity','Assets/Scenes/ThirdDrain.unity.meta','ProjectSettings/EditorBuildSettings.asset','ProjectSettings/TagManager.asset')){Copy-Item -LiteralPath (Join-Path $taskBuildProject $taskFile) -Destination (Join-Path $taskProject $taskFile) -Force}
 foreach($taskFile in @('Assets/Prefabs/WickSpirit.prefab','Assets/Prefabs/WickSpirit.prefab.meta')){Copy-Item -LiteralPath (Join-Path $taskBuildProject $taskFile) -Destination (Join-Path $taskProject $taskFile) -Force}
}
Get-Content -LiteralPath (Join-Path $taskValidation 'build-result.json')
if($Validate){
 $taskOut=Join-Path $taskValidation 'native-run'
 [IO.Directory]::CreateDirectory($taskOut) | Out-Null
 $taskPlayerArgs=@('--validate-level03','-validationOutput',('"'+$taskOut+'"'),'-logFile',('"'+(Join-Path $taskOut 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810')
 if($CaptureWaterMotion){$taskPlayerArgs+='--capture-water-motion'}
 if($CaptureRig){$taskPlayerArgs+='--capture-rig'}
 $taskPlayer=Start-Process -FilePath (Join-Path $taskWorkspace 'Builds/Windows-Level03/LightFeast.exe') -ArgumentList $taskPlayerArgs -WindowStyle Hidden -PassThru
 $taskPlayer.WaitForExit()
 $taskReport=Get-Content -LiteralPath (Join-Path $taskOut 'playthrough.json') -Raw | ConvertFrom-Json
 $taskReport | Select-Object completed,seconds,jumps,deaths
 $taskReport.checks | Where-Object {-not $_.passed}
 if($taskPlayer.ExitCode -ne 0){throw 'Native third-level test failed; see Validation/level03/native-run/playthrough.json'}
}
