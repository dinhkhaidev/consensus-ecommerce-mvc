param(
    [string]$Root = (Resolve-Path "$PSScriptRoot\..").Path
)

$ErrorActionPreference = "Stop"

$assetRoot = Join-Path $Root "wwwroot\models\demo-products\sharetextures"
$tempRoot = Join-Path $Root "wwwroot\models\_downloads\sharetextures"
New-Item -ItemType Directory -Force -Path $assetRoot | Out-Null
New-Item -ItemType Directory -Force -Path $tempRoot | Out-Null

$assets = @(
    @{ Key = "sofa-22"; Slug = "sofa-22"; Category = "sofa"; Source = "https://www.sharetextures.com/models/furniture/sofa-22" },
    @{ Key = "sofa-21"; Slug = "sofa-21"; Category = "sofa"; Source = "https://www.sharetextures.com/models/furniture/sofa-21" },
    @{ Key = "armchair-16"; Slug = "armchair-16"; Category = "chair"; Source = "https://www.sharetextures.com/models/furniture/armchair-16" },
    @{ Key = "armchair-15"; Slug = "armchair-15"; Category = "chair"; Source = "https://www.sharetextures.com/models/furniture/armchair-15" },
    @{ Key = "armchair-14"; Slug = "armchair-14"; Category = "chair"; Source = "https://www.sharetextures.com/models/furniture/armchair-14" },
    @{ Key = "armchair-13"; Slug = "armchair-13"; Category = "chair"; Source = "https://www.sharetextures.com/models/furniture/armchair-13" },
    @{ Key = "table-5"; Slug = "table-5"; Category = "table"; Source = "https://www.sharetextures.com/models/furniture/table-5" },
    @{ Key = "table-4"; Slug = "table-4"; Category = "table"; Source = "https://www.sharetextures.com/models/furniture/table-4" },
    @{ Key = "stool-9"; Slug = "stool-9"; Category = "chair"; Source = "https://www.sharetextures.com/models/furniture/stool-9" }
)

function Invoke-CurlDownload {
    param(
        [string]$Url,
        [string]$Path
    )

    $dir = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    & curl.exe -L --fail --silent --show-error --retry 2 --max-time 240 -o $Path $Url
    if ($LASTEXITCODE -ne 0) {
        throw "Download failed: $Url"
    }
}

function Find-Link {
    param(
        [string]$Html,
        [string]$Title
    )

    $directPattern = '"title":"' + [regex]::Escape($Title) + '","value":"(https://files\.sharetextures\.com/[^"]+)"'
    $match = [regex]::Match($Html, $directPattern)
    if ($match.Success) {
        return ($match.Groups[1].Value -replace '\\u0026', '&')
    }

    return $null
}

function Find-Preview {
    param([string]$Html)

    $match = [regex]::Match($Html, '<meta name="tex1:preview-image" content="([^"]+)"')
    if ($match.Success) {
        return $match.Groups[1].Value
    }

    return $null
}

$manifest = @()

foreach ($asset in $assets) {
    try {
        Write-Host "Fetching $($asset.Key)"
        $htmlPath = Join-Path $tempRoot "$($asset.Key).html"
        Invoke-CurlDownload -Url $asset.Source -Path $htmlPath
        $html = Get-Content $htmlPath -Raw

        $fbxUrl = Find-Link -Html $html -Title "FBX"
        $textureUrl = Find-Link -Html $html -Title "1K Textures"
        $previewUrl = Find-Preview -Html $html

        if ([string]::IsNullOrWhiteSpace($fbxUrl)) {
            throw "No direct FBX link found."
        }

        $targetDir = Join-Path $assetRoot $asset.Key
        Remove-Item -LiteralPath $targetDir -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

        $fbxFile = Join-Path $targetDir "$($asset.Key).fbx"
        Invoke-CurlDownload -Url $fbxUrl -Path $fbxFile

        if (-not [string]::IsNullOrWhiteSpace($textureUrl)) {
            $zipPath = Join-Path $tempRoot "$($asset.Key)-1k.zip"
            Invoke-CurlDownload -Url $textureUrl -Path $zipPath
            Expand-Archive -LiteralPath $zipPath -DestinationPath $targetDir -Force
            Remove-Item -LiteralPath $zipPath -Force
        }

        $previewPath = $null
        if (-not [string]::IsNullOrWhiteSpace($previewUrl)) {
            $previewFile = Join-Path $targetDir "$($asset.Key)-preview.webp"
            Invoke-CurlDownload -Url $previewUrl -Path $previewFile
            $previewPath = $previewFile.Substring((Join-Path $Root "wwwroot").Length).Replace("\", "/")
        }

        $modelPath = $fbxFile.Substring((Join-Path $Root "wwwroot").Length).Replace("\", "/")
        $manifest += [pscustomobject]@{
            key = $asset.Key
            category = $asset.Category
            modelUrl = $modelPath
            thumbnail = $previewPath
            format = "fbx"
            source = $asset.Source
            license = "CC0"
        }

        Write-Host "Downloaded $($asset.Key) -> $modelPath"
    }
    catch {
        Write-Warning "Could not download $($asset.Key): $($_.Exception.Message)"
    }
}

$manifestPath = Join-Path $assetRoot "room3d-sharetextures.json"
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath -Encoding UTF8
Write-Host "Manifest written: $manifestPath"
