param([switch]$PlayerOnly,[switch]$Validate,[switch]$ShowPlayer,[switch]$ValidateOnly,[switch]$RegressionFifth)
$ErrorActionPreference='Stop'
$sixWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sixProject=Join-Path $sixWorkspace 'UnityProject-NOVA-URP'
$sixOutput=Join-Path $sixWorkspace 'Validation/level06'
[IO.Directory]::CreateDirectory($sixOutput) | Out-Null
if(-not $ValidateOnly -and -not $RegressionFifth){
 $sixOpen=Get-Process Unity -ErrorAction SilentlyContinue
 if($sixOpen){throw 'Save and close the open Unity editor before batch building. No editor will be closed automatically.'}
 $sixMethod=if($PlayerOnly){'SixthLevelBuilder.BuildPlayer'}else{'SixthLevelBuilder.BuildAll'}
 $sixProcess=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$sixProject+'"'),'-executeMethod',$sixMethod,'-logFile',('"'+(Join-Path $sixOutput 'build.log')+'"')) -WindowStyle Hidden -PassThru
 $sixProcess.WaitForExit()
 if($sixProcess.ExitCode -ne 0){throw 'Sixth level build failed. See Validation/level06/build.log'}
 Get-Content -LiteralPath (Join-Path $sixOutput 'build-result.json')
}
if($RegressionFifth){
 $fiveNative=Join-Path $sixWorkspace 'Validation/level05/native-run'
 $fiveBackup=Join-Path $sixOutput 'fifth-before'
 if((Test-Path -LiteralPath $fiveNative) -and -not(Test-Path -LiteralPath $fiveBackup)){
  Copy-Item -LiteralPath $fiveNative -Destination $fiveBackup -Recurse
 }
 $fiveStyle=if($ShowPlayer){'Normal'}else{'Hidden'}
 $fiveRun=Start-Process -FilePath (Join-Path $sixWorkspace 'Builds/Windows-Level06-NOVA/LightFeast.exe') -ArgumentList @('--validate-level05','-logFile',('"'+(Join-Path $sixOutput 'FifthRegression.log')+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900') -WindowStyle $fiveStyle -PassThru
 $fiveRun.WaitForExit()
 $fiveReport=Join-Path $fiveNative 'playthrough.json'
 $fiveResult=Get-Content -Raw -LiteralPath $fiveReport | ConvertFrom-Json
 $fiveDestination=Join-Path $sixOutput 'fifth-regression'
 [IO.Directory]::CreateDirectory($fiveDestination) | Out-Null
 Get-ChildItem -LiteralPath $fiveNative -File | Copy-Item -Destination $fiveDestination -Force
 [pscustomobject]@{completed=$fiveResult.completed;checks=$fiveResult.checks.Count;failed=@($fiveResult.checks | Where-Object {-not $_.passed}).Count;errors=$fiveResult.errors.Count;jumps=$fiveResult.jumps;deaths=$fiveResult.deaths;transformations=$fiveResult.transformations} | ConvertTo-Json
 if($fiveRun.ExitCode -ne 0 -or -not $fiveResult.completed){throw 'Fifth-level regression failed.'}
}
if($Validate -or $ValidateOnly){
 $sixStyle=if($ShowPlayer){'Normal'}else{'Hidden'}
 $sixPlayer=Start-Process -FilePath (Join-Path $sixWorkspace 'Builds/Windows-Level06-NOVA/LightFeast.exe') -ArgumentList @('--validate-level06','-logFile',('"'+(Join-Path $sixOutput 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1600','-screen-height','900') -WindowStyle $sixStyle -PassThru
 $sixPlayer.WaitForExit()
 Get-Content -LiteralPath (Join-Path $sixOutput 'native-run/playthrough.json')
 if($sixPlayer.ExitCode -ne 0){throw 'Sixth level input validation failed.'}
}
