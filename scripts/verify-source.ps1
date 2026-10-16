$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$required = @('README.md','LICENSE','NOTICE.md','OrbitLauncher.csproj','Core\Orbit.Core.csproj','ThirdParty\authlib-injector-1.2.8.jar','ThirdParty\authlib-source-1.2.8.zip')
foreach ($relative in $required) {
    if (-not (Test-Path -LiteralPath (Join-Path $projectRoot $relative) -PathType Leaf)) { throw ('Missing source file: ' + $relative) }
}
$agent = Join-Path $projectRoot 'ThirdParty\authlib-injector-1.2.8.jar'
$expected = '9c7f4343e6c82034958ffb48c14a2cb0c85928be7283103ce17da00c6d5a7b10'
if ((Get-FileHash -LiteralPath $agent -Algorithm SHA256).Hash.ToLowerInvariant() -ne $expected) { throw 'Pinned skin agent hash mismatch.' }
$checked = 0
foreach ($file in (Get-ChildItem -LiteralPath $projectRoot -Recurse -File -Force)) {
    $relative = $file.FullName.Substring($projectRoot.Length + 1).Replace('\','/')
    if ($relative -match '(^|/)(\.git|bin|obj|artifacts|release[^/]*)(/|$)') { continue }
    if ($relative -match '(^|/)(settings\.json|latest-launch\.log|skin\.png|selected\.png)$' -or $file.Extension -in @('.exe','.dll','.pdb','.pfx','.p12','.key','.pem','.snk')) { throw ('Private/build file in source tree: ' + $relative) }
    if ($file.Extension -in @('.jar','.zip') -and $relative -notin @('ThirdParty/authlib-injector-1.2.8.jar','ThirdParty/authlib-source-1.2.8.zip')) { throw ('Unexpected binary archive: ' + $relative) }
    if ($file.Extension -in @('.cs','.xaml','.csproj','.java','.md','.json','.yml','.yaml')) {
        $text = [IO.File]::ReadAllText($file.FullName)
        if ($text -match '-----BEGIN (RSA |EC |OPENSSH |ENCRYPTED )?PRIVATE KEY-----') { throw ('Private key marker: ' + $relative) }
        if ($text -match '(?i)[A-Z]:\\Users\\[^\\\s]+\\') { throw ('Personal absolute path: ' + $relative) }
        if ($text -match '(?i)(api[_-]?(hash|key)|client[_-]?secret|refresh[_-]?token|password)\s*[:=]\s*["''][a-zA-Z0-9_\-]{20,}["'']') { throw ('Possible embedded credential: ' + $relative) }
    }
    $checked++
}
Write-Output ('PASS source inventory and basic secret/path checks: ' + $checked + ' files.')
Write-Output 'PASS pinned authlib-injector checksum.'
