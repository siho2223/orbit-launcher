param([switch]$Publish,[switch]$Test)
$ErrorActionPreference = 'Stop'
$projectRoot = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
Push-Location $projectRoot
try {
    & dotnet build .\OrbitLauncher.csproj -c Release --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
    if ($Test) {
        & dotnet run --project .\Tests -c Release
        if ($LASTEXITCODE -ne 0) { throw 'Tests failed.' }
    }
    if ($Publish) {
        $publishPath = Join-Path $projectRoot 'artifacts\publish\win-x64'
        & dotnet publish .\OrbitLauncher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o $publishPath --nologo
        if ($LASTEXITCODE -ne 0) { throw 'Publish failed.' }
        foreach ($name in @('README.md','LICENSE','NOTICE.md')) {
            Copy-Item -LiteralPath (Join-Path $projectRoot $name) -Destination $publishPath
        }
        Copy-Item -LiteralPath (Join-Path $projectRoot 'docs') -Destination $publishPath -Recurse -Force
        $noticePath = Join-Path $publishPath 'ThirdParty'
        New-Item -ItemType Directory -Path $noticePath -Force | Out-Null
        Get-ChildItem -LiteralPath (Join-Path $projectRoot 'ThirdParty') -File |
            Where-Object Extension -ne '.jar' |
            ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination $noticePath }
        Write-Output ('Published: ' + $publishPath)
    }
} finally {
    Pop-Location
}
