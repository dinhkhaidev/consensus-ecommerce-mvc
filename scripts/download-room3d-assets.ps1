param(
    [string]$Root = (Resolve-Path "$PSScriptRoot\..").Path,
    [string]$Resolution = "1k"
)

$ErrorActionPreference = "Stop"

$assetRoot = Join-Path $Root "wwwroot\models\demo-products"
New-Item -ItemType Directory -Force -Path $assetRoot | Out-Null

$assets = @(
    @{ Key = "sofa"; AssetId = "sofa_02"; ProductUrl = "https://polyhaven.com/a/sofa_02" },
    @{ Key = "coffee-table"; AssetId = "modern_coffee_table_01"; ProductUrl = "https://polyhaven.com/a/modern_coffee_table_01" },
    @{ Key = "lounge-chair"; AssetId = "ArmChair_01"; ProductUrl = "https://polyhaven.com/a/ArmChair_01" },
    @{ Key = "plant"; AssetId = "potted_plant_04"; ProductUrl = "https://polyhaven.com/a/potted_plant_04" },
    @{ Key = "tv-stand"; AssetId = "drawer_cabinet"; ProductUrl = "https://polyhaven.com/a/drawer_cabinet" }
)

function Get-JsonFromUrl {
    param([string]$Url)

    $json = & curl.exe -L --fail --silent --show-error --max-time 60 $Url
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($json)) {
        throw "Could not fetch $Url"
    }

    return ($json | ConvertFrom-Json)
}

function Save-Url {
    param(
        [string]$Url,
        [string]$Path
    )

    $dir = Split-Path -Parent $Path
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    & curl.exe -L --fail --silent --show-error --retry 2 --max-time 180 -o $Path $Url
    if ($LASTEXITCODE -ne 0) {
        throw "Download failed: $Url"
    }
}

function Get-GltfEntry {
    param(
        [object]$Files,
        [string]$PreferredResolution
    )

    $resolutions = @($PreferredResolution, "1k", "2k", "4k") | Select-Object -Unique
    foreach ($res in $resolutions) {
        $resNode = $Files.gltf.PSObject.Properties[$res]
        if ($null -eq $resNode) {
            continue
        }

        $entry = $resNode.Value.PSObject.Properties["gltf"]
        if ($null -ne $entry) {
            return @{ Resolution = $res; Entry = $entry.Value }
        }
    }

    return $null
}

$manifest = @()

foreach ($asset in $assets) {
    $targetDir = Join-Path $assetRoot $asset.Key

    try {
        Write-Host "Fetching metadata for $($asset.Key) ($($asset.AssetId))"
        $files = Get-JsonFromUrl "https://api.polyhaven.com/files/$($asset.AssetId)"
        $gltfInfo = Get-GltfEntry -Files $files -PreferredResolution $Resolution

        if ($null -eq $gltfInfo) {
            throw "No glTF entry found."
        }

        Remove-Item -LiteralPath $targetDir -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Force -Path $targetDir | Out-Null

        $gltfName = Split-Path $gltfInfo.Entry.url -Leaf
        $gltfPath = Join-Path $targetDir $gltfName
        Save-Url -Url $gltfInfo.Entry.url -Path $gltfPath

        foreach ($include in $gltfInfo.Entry.include.PSObject.Properties) {
            $includePath = Join-Path $targetDir ($include.Name -replace "/", "\")
            Save-Url -Url $include.Value.url -Path $includePath
        }

        $relative = $gltfPath.Substring((Join-Path $Root "wwwroot").Length).Replace("\", "/")
        $manifest += [pscustomobject]@{
            key = $asset.Key
            url = $relative
            resolution = $gltfInfo.Resolution
            source = $asset.ProductUrl
            license = "CC0"
        }

        Write-Host "Downloaded $($asset.Key) -> $relative"
    }
    catch {
        Write-Warning "Could not download $($asset.Key): $($_.Exception.Message)"
    }
}

$manifestPath = Join-Path $assetRoot "room3d-models.json"
$manifest | ConvertTo-Json -Depth 4 | Set-Content -Path $manifestPath -Encoding UTF8
Write-Host "Manifest written: $manifestPath"
