param([switch]$PlayerOnly,[switch]$Validate,[switch]$ShowPlayer,[switch]$ValidateOnly,[switch]$RegressionSixth)
$ErrorActionPreference='Stop'
$sevenWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sevenProject=Join-Path $sevenWorkspace 'UnityProject-NOVA-URP'
$sevenOutput=Join-Path $sevenWorkspace 'Validation/level07'
[IO.Directory]::CreateDirectory($sevenOutput) | Out-Null
if(-not $ValidateOnly -and -not $RegressionSixth){
 if(Get-Process Unity -ErrorAction SilentlyContinue){throw 'Save and close the Unity editor before batch building. No editor will be closed automatically.'}
 $sevenMethod=if($PlayerOnly){'SeventhLevelBuilder.BuildPlayer'}else{'SeventhLevelBuilder.BuildAll'}
 $sevenProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$sevenProject+'"'),'-executeMethod',$sevenMethod,'-logFile',('"'+(Join-Path $sevenOutput 'build.log')+'"')) -WindowStyle Hidden -PassThru
 if(-not $sevenProcess.WaitForExit(900000)){Stop-Process -Id $sevenProcess.Id;throw 'Seventh-level batch build exceeded 15 minutes.'}
 if($sevenProcess.ExitCode -ne 0){throw 'Seventh-level build failed. See Validation/level07/build.log'}
 Get-Content -LiteralPath (Join-Path $sevenOutput 'build-result.json')
}
if($Validate -or $ValidateOnly -or $RegressionSixth){
 $sevenStyle=if($ShowPlayer){'Normal'}else{'Hidden'}
 $sevenLevel=if($RegressionSixth){'06'}else{'07'}
 $sevenNative=Join-Path $sevenWorkspace ('Validation/level'+$sevenLevel+'/native-run')
 if($RegressionSixth -and (Test-Path -LiteralPath $sevenNative)){
  $sevenBackup=Join-Path $sevenOutput 'sixth-before'
  if(-not(Test-Path -LiteralPath $sevenBackup)){Copy-Item -LiteralPath $sevenNative -Destination $sevenBackup -Recurse}
 }
 $sevenLog=if($RegressionSixth){'SixthRegression.log'}else{'Player.log'}
 $sevenPlayer=Start-Process -FilePath (Join-Path $sevenWorkspace 'Builds/Windows-Level07-NOVA/LightFeast.exe') -ArgumentList @(('--validate-level'+$sevenLevel),'-logFile',('"'+(Join-Path $sevenOutput $sevenLog)+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900') -WindowStyle $sevenStyle -PassThru
 if(-not $sevenPlayer.WaitForExit(480000)){Stop-Process -Id $sevenPlayer.Id;throw 'Native input validation exceeded eight minutes.'}
 $sevenReport=Get-Content -Raw -LiteralPath (Join-Path $sevenNative 'playthrough.json') | ConvertFrom-Json
 if($RegressionSixth){
  $sevenDestination=Join-Path $sevenOutput 'sixth-regression'
  [IO.Directory]::CreateDirectory($sevenDestination) | Out-Null
  Get-ChildItem -LiteralPath $sevenNative -File | Copy-Item -Destination $sevenDestination -Force
 }
 [pscustomobject]@{level=$sevenLevel;completed=$sevenReport.completed;assertions=$sevenReport.checks.Count;failed=@($sevenReport.checks | Where-Object {-not $_.passed});errors=$sevenReport.errors;jumps=$sevenReport.jumps;deaths=$sevenReport.deaths;transformations=$sevenReport.transformations;action=$sevenReport.action} | ConvertTo-Json -Depth 5
 if($sevenPlayer.ExitCode -ne 0 -or -not $sevenReport.completed){throw 'Native input validation failed. See the saved report and player log.'}
}
