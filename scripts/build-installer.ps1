param(
    [string]$Iscc = 'ISCC.exe',
    [string]$PublishDir,
    [string]$OutputDir
)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if (-not $PublishDir) {
    & (Join-Path $PSScriptRoot 'build.ps1') -Publish -Test
    $PublishDir = Join-Path $projectRoot 'artifacts\publish\win-x64'
}
$PublishDir = (Resolve-Path -LiteralPath $PublishDir).Path
if (-not (Test-Path -LiteralPath (Join-Path $PublishDir 'OrbitLauncher.exe'))) { throw 'Missing published launcher executable.' }
if (-not $OutputDir) { $OutputDir = Join-Path $projectRoot 'artifacts\installer' }
New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
$OutputDir = (Resolve-Path -LiteralPath $OutputDir).Path
$payload = Join-Path $projectRoot ('artifacts\installer-payload-' + [guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $payload | Out-Null
# Keep the payload allowlisted: never package build symbols, profiles or game files.
Copy-Item -LiteralPath (Join-Path $PublishDir 'OrbitLauncher.exe') -Destination $payload -Force
foreach ($name in @('README.md','LICENSE','NOTICE.md')) {
    Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $payload -Force
}
foreach ($name in @('docs','ThirdParty')) {
    $dest = Join-Path $payload $name
    New-Item -ItemType Directory -Force -Path $dest | Out-Null
    if ($name -eq 'docs') {
        Get-ChildItem -LiteralPath (Join-Path $projectRoot $name) -File | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $dest -Force }
    } else {
        Get-ChildItem -LiteralPath (Join-Path $projectRoot $name) -File | Where-Object Extension -ne '.jar' | ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $dest -Force }
    }
}
[xml]$project = Get-Content -LiteralPath (Join-Path $projectRoot 'OrbitLauncher.csproj')
$version = [string]$project.Project.PropertyGroup.Version
& $Iscc ('/DAppVersion=' + $version) ('/DPayloadDir=' + $payload) ('/DInstallerOutput=' + $OutputDir) (Join-Path $projectRoot 'installer\ORBIT.iss')
if ($LASTEXITCODE -ne 0) { throw 'Installer compilation failed.' }
$setup = Join-Path $OutputDir ('ORBIT-Setup-' + $version + '-Windows-x64.exe')
$hash = (Get-FileHash -LiteralPath $setup -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText(($setup + '.sha256'), ($hash + '  ' + [IO.Path]::GetFileName($setup) + "`n"))
Write-Output ('Installer: ' + $setup)
