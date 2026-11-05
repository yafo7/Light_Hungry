param([switch]$Review,[switch]$PlayerOnly)
$ErrorActionPreference='Stop'
$taskWorkspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$taskProject=Join-Path $taskWorkspace 'UnityProject-NOVA-URP'
$taskEvidence=Join-Path $taskWorkspace 'Validation/background-watercolor-v01'
[IO.Directory]::CreateDirectory($taskEvidence) | Out-Null
$taskOpen=Get-CimInstance Win32_Process -Filter "Name='Unity.exe'" | Where-Object {$_.CommandLine -like ('*'+$taskProject+'*')}
if($taskOpen){throw 'Save and close the NOVA Unity editor before this batch build.'}
$taskMethod=if($PlayerOnly){'BackgroundStyleReview.BuildPlayer'}else{'BackgroundStyleReview.BuildAll'}
$taskPrevious=$env:LIGHTFEAST_WORKSPACE
try {
 $env:LIGHTFEAST_WORKSPACE=$taskWorkspace
 $taskEditor=Start-Process -FilePath 'C:/Program Files/Unity/Hub/Editor/2021.3.42f1c1/Editor/Unity.exe' -ArgumentList @('-batchmode','-quit','-projectPath',('"'+$taskProject+'"'),'-executeMethod',$taskMethod,'-logFile',('"'+(Join-Path $taskEvidence 'build.log')+'"')) -WindowStyle Hidden -PassThru
 $taskEditor.WaitForExit()
 if($taskEditor.ExitCode -ne 0){throw 'Watercolor background build failed; see Validation/background-watercolor-v01/build.log'}
} finally {if($null -eq $taskPrevious){Remove-Item Env:LIGHTFEAST_WORKSPACE -ErrorAction SilentlyContinue}else{$env:LIGHTFEAST_WORKSPACE=$taskPrevious}}
Get-Content -LiteralPath (Join-Path $taskEvidence 'build-result.json')
if($Review){
 $taskOutput=Join-Path $taskEvidence 'native-review'
 [IO.Directory]::CreateDirectory($taskOutput) | Out-Null
 $taskPlayer=Start-Process -FilePath (Join-Path $taskWorkspace 'Builds/Windows-Watercolor-NOVA/LightFeast.exe') -ArgumentList @('--review-backgrounds','-validationOutput',('"'+$taskOutput+'"'),'-logFile',('"'+(Join-Path $taskOutput 'Player.log')+'"'),'-screen-fullscreen','0','-screen-width','1440','-screen-height','810') -WindowStyle Normal -PassThru
 $taskPlayer.WaitForExit()
 Get-Content -LiteralPath (Join-Path $taskOutput 'review.json') -Raw | ConvertFrom-Json | Select-Object completed,@{Name='views';Expression={$_.views.Count}},@{Name='errors';Expression={$_.errors.Count}} | ConvertTo-Json
 if($taskPlayer.ExitCode -ne 0){throw 'Native background review failed; see Validation/background-watercolor-v01/native-review/review.json'}
}
