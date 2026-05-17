param(
    [string]$SketchfabToken = $env:SKETCHFAB_TOKEN
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($SketchfabToken)) {
    throw "Missing Sketchfab token. Pass -SketchfabToken or set SKETCHFAB_TOKEN."
}

$root = Split-Path -Parent $PSScriptRoot
$decorRoot = Join-Path $root "wwwroot\models\rooms\decor"
New-Item -ItemType Directory -Force -Path $decorRoot | Out-Null

$models = @(
    @{
        Name = "shenron-yanez"
        Uid = "96e8ad1e206941ce859c5733bec75d30"
    },
    @{
        Name = "katana-wall"
        Uid = "aec45be9d79d4c0885c2ec7fb106fd4b"
    }
)

$headers = @{
    Authorization = "Bearer $SketchfabToken"
}

foreach ($model in $models) {
    Write-Host "Fetching download URL for $($model.Name)..."
    $download = Invoke-RestMethod `
        -Uri "https://api.sketchfab.com/v3/models/$($model.Uid)/download" `
        -Headers $headers

    if (-not $download.gltf.url) {
        throw "Sketchfab did not return a glTF download URL for $($model.Name)."
    }

    $zipPath = Join-Path $decorRoot "$($model.Name).zip"
    $extractPath = Join-Path $decorRoot $model.Name

    Write-Host "Downloading $($model.Name)..."
    Invoke-WebRequest -Uri $download.gltf.url -OutFile $zipPath

    if (Test-Path $extractPath) {
        Remove-Item -LiteralPath $extractPath -Recurse -Force
    }

    New-Item -ItemType Directory -Force -Path $extractPath | Out-Null
    Expand-Archive -LiteralPath $zipPath -DestinationPath $extractPath -Force
    Remove-Item -LiteralPath $zipPath -Force

    $gltf = Get-ChildItem -Path $extractPath -Recurse -Filter "*.gltf" | Select-Object -First 1
    if (-not $gltf) {
        throw "Downloaded $($model.Name), but no .gltf file was found."
    }

    if ($gltf.Name -ne "scene.gltf" -or $gltf.DirectoryName -ne $extractPath) {
        Copy-Item -LiteralPath $gltf.FullName -Destination (Join-Path $extractPath "scene.gltf") -Force
    }

    Write-Host "Ready: $($model.Name)/scene.gltf"
}
