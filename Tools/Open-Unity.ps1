$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$project = Join-Path $workspace 'UnityProject'
$editor = 'C:\Program Files\Unity\Hub\Editor\2021.3.42f1c1\Editor\Unity.exe'
Start-Process -FilePath $editor -ArgumentList @('-projectPath',('"'+$project+'"'),'-executeMethod','FirstLevelReview.Open')
