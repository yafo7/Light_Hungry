param([ValidateSet('Inspect','Network','Prepare','Stage','Commit','Push','Verify')][string]$Action='Inspect')
$ErrorActionPreference='Stop'
$syncRoot=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
Set-Location -LiteralPath $syncRoot
$syncRemote='https://github.com/yafo7/Light_Hungry.git'
$syncVersion='v0.7.0-alpha.1'
$syncBranch='codex/sync-v0.7.0-alpha.1'
$syncEvidence=Join-Path $syncRoot 'Validation/github-sync-v0.7.0-alpha.1'
[IO.Directory]::CreateDirectory($syncEvidence) | Out-Null
function Run-Git([string[]]$GitArgs) { & git @GitArgs; if($LASTEXITCODE -ne 0){throw ('git failed: '+($GitArgs -join ' '))} }
function Get-RepoAccess {
 $syncCredential = "protocol=https`nhost=github.com`n`n" | git -c credential.interactive=never credential fill 2>$null
 if($LASTEXITCODE -ne 0){throw 'No usable GitHub credential is available in the local credential manager.'}
 $syncPassword=($syncCredential | Where-Object {$_ -like 'password=*'} | Select-Object -First 1)
 if(-not $syncPassword){throw 'Credential manager returned no GitHub credential.'}
 $syncHeaders=@{Authorization=('Bearer '+$syncPassword.Substring(9));Accept='application/vnd.github+json';'User-Agent'='LightHungry-RepositorySync'}
 $syncRepo=Invoke-RestMethod -Uri 'https://api.github.com/repos/yafo7/Light_Hungry' -Headers $syncHeaders
 $syncUser=Invoke-RestMethod -Uri 'https://api.github.com/user' -Headers $syncHeaders
 $syncAccess=[ordered]@{login=$syncUser.login;id=$syncUser.id;repository=$syncRepo.full_name;defaultBranch=$syncRepo.default_branch;push=$syncRepo.permissions.push;visibility=$syncRepo.visibility}
 $syncAccess | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $syncEvidence 'access.json') -Encoding UTF8
 $syncCredential=$null;$syncPassword=$null;$syncHeaders=$null
 return $syncAccess
}
if($Action -eq 'Inspect') { Get-RepoAccess | ConvertTo-Json; exit }
if($Action -eq 'Network') {
 Get-NetTCPConnection -State Listen | Where-Object {$_.LocalPort -in @(7890,7897,1080,10808,10809)} | Select-Object LocalAddress,LocalPort
 $syncInternet=Get-ItemProperty -LiteralPath 'HKCU:/Software/Microsoft/Windows/CurrentVersion/Internet Settings'
 [pscustomobject]@{ProxyEnabled=$syncInternet.ProxyEnable;ProxyServer=$syncInternet.ProxyServer}
 exit
}
if($Action -eq 'Prepare') {
 $syncRemotes=@(git remote)
 if($syncRemotes -notcontains 'origin'){Run-Git -GitArgs @('remote','add','origin',$syncRemote)} elseif((git remote get-url origin) -ne $syncRemote){throw 'An unexpected origin exists; leave it unchanged.'}
 Run-Git -GitArgs @('config','--local','remote.origin.promisor','true')
 Run-Git -GitArgs @('config','--local','remote.origin.partialclonefilter','blob:none')
 Run-Git -GitArgs @('config','--local','gc.auto','0')
 Run-Git -GitArgs @('config','--local','maintenance.auto','false')
 $syncBase=git rev-parse --verify --quiet origin/main
 if($LASTEXITCODE -ne 0){Run-Git -GitArgs @('-c','http.version=HTTP/1.1','-c','http.lowSpeedLimit=4096','-c','http.lowSpeedTime=45','fetch','--no-auto-maintenance','--filter=blob:none','origin','main','--tags')}
 # The public connector supplied these four exact blobs. Verify object hashes
 # before using them to avoid re-downloading unrelated historical art.
 $syncBaseFiles=Join-Path $syncEvidence 'remote-base/files.json'
 if(Test-Path -LiteralPath $syncBaseFiles){
  foreach($syncFile in (Get-Content -LiteralPath $syncBaseFiles -Raw | ConvertFrom-Json)){
   $syncHash=git hash-object -w --no-filters -- (Join-Path (Join-Path $syncEvidence 'remote-base') $syncFile.name)
   if($LASTEXITCODE -ne 0 -or $syncHash -ne $syncFile.sha){throw ('Remote blob hash mismatch: '+$syncFile.name)}
  }
 }
 $syncHead=git rev-parse --verify --quiet HEAD
 if($LASTEXITCODE -eq 0){throw 'Prepare expects the original unborn local branch; inspect existing history before continuing.'}
 $syncIgnore=Join-Path $syncRoot '.gitignore'
 $syncBackup=Join-Path $syncEvidence 'original.gitignore'
 if(Test-Path -LiteralPath $syncIgnore){if(Test-Path -LiteralPath $syncBackup){throw 'Ignore backup already exists.'};Move-Item -LiteralPath $syncIgnore -Destination $syncBackup}
 Run-Git -GitArgs @('config','--local','core.sparseCheckout','true')
 [IO.Directory]::CreateDirectory((Join-Path $syncRoot '.git/info')) | Out-Null
 [IO.File]::WriteAllLines((Join-Path $syncRoot '.git/info/sparse-checkout'), @('/*','!/Assets/','!/Documentation/','!/generated/','!/Packages/','!/ProjectSettings/'), (New-Object System.Text.UTF8Encoding($false)))
 try {Run-Git -GitArgs @('-c','http.version=HTTP/1.1','switch','-c',$syncBranch,'origin/main')} catch {if(-not(Test-Path -LiteralPath $syncIgnore)){Copy-Item -LiteralPath $syncBackup -Destination $syncIgnore};throw}
 $syncAccess=Get-RepoAccess
 Run-Git -GitArgs @('config','--local','user.name',$syncAccess.login)
 Run-Git -GitArgs @('config','--local','user.email',($syncAccess.id.ToString()+'+'+$syncAccess.login+'@users.noreply.github.com'))
 $syncAccess | ConvertTo-Json
 exit
}
if($Action -eq 'Stage') {
 Run-Git -GitArgs @('lfs','install','--local')
 Run-Git -GitArgs @('add','--','.gitignore','.gitattributes','README.md','VERSION','CHANGELOG.md','GAME_DESIGN.md','Tools','UnityProject','UnityProject-NOVA-URP','art')
 Get-ChildItem -LiteralPath $syncRoot -File -Filter '*.cmd' | ForEach-Object {Run-Git -GitArgs @('add','--',$_.Name)}
 Get-ChildItem -LiteralPath $syncRoot -File -Filter '*_basecolor.jpg' | ForEach-Object {Run-Git -GitArgs @('add','--',$_.Name)}
 Run-Git -GitArgs @('diff','--cached','--shortstat')
 exit
}
if($Action -eq 'Commit') {
 Run-Git -GitArgs @('commit','-F',(Join-Path $syncEvidence 'commit-message.txt'))
 Run-Git -GitArgs @('tag','-a',$syncVersion,'-F',(Join-Path $syncEvidence 'tag-message.txt'))
 Run-Git -GitArgs @('show','--no-patch','--format=fuller','HEAD')
 exit
}
if($Action -eq 'Push') {
 Run-Git -GitArgs @('-c','http.version=HTTP/1.1','-c','http.lowSpeedLimit=4096','-c','http.lowSpeedTime=45','fetch','--no-auto-maintenance','origin','main','--tags')
 Run-Git -GitArgs @('merge-base','--is-ancestor','origin/main','HEAD')
 Run-Git -GitArgs @('-c','http.version=HTTP/1.1','push','--atomic','origin','HEAD:refs/heads/main',('refs/tags/'+$syncVersion))
 Run-Git -GitArgs @('branch','--set-upstream-to=origin/main',$syncBranch)
 exit
}
if($Action -eq 'Verify') {
 Run-Git -GitArgs @('ls-remote','origin','refs/heads/main',('refs/tags/'+$syncVersion),('refs/tags/'+$syncVersion+'^{}'))
 Run-Git -GitArgs @('rev-parse','HEAD')
 Run-Git -GitArgs @('status','--short','--branch')
}
