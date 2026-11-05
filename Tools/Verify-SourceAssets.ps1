$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$original = Join-Path $workspace 'art\levels01-02-micro-scale-v04-2026-10-30'
$imported = Join-Path $workspace 'UnityProject\Assets\Art\Level01'
$rows = @()
foreach ($file in @('l1-background.png','l1-ground.png','reflector.png','tail-rubble.png','pot.png','assets.json')) {
    $source = Get-FileHash -LiteralPath (Join-Path $original $file) -Algorithm SHA256
    $dest = Get-FileHash -LiteralPath (Join-Path $imported $file) -Algorithm SHA256
    $rows += [pscustomobject]@{ asset=$file; identical=($source.Hash -eq $dest.Hash); sha256=$dest.Hash }
}
$model = 'C:\Users\10029\Desktop\食光兽\美术\卡通人物3d模型.glb'
$modelCopy = Join-Path $workspace 'UnityProject\Assets\Models\Source\WickSpirit.original.glb'
$sourceHash = (Get-FileHash -LiteralPath $model -Algorithm SHA256).Hash
$copyHash = (Get-FileHash -LiteralPath $modelCopy -Algorithm SHA256).Hash
$rows += [pscustomobject]@{ asset='WickSpirit.original.glb'; identical=($sourceHash -eq $copyHash); sha256=$copyHash }
$rows | ConvertTo-Json -Depth 3 | Set-Content -LiteralPath (Join-Path $workspace 'Validation\source-assets.json') -Encoding UTF8
$rows | Format-Table -AutoSize
if ($rows.identical -contains $false) { throw 'Source asset bytes differ.' }
