#Requires -Version 5.0
<#
.SYNOPSIS
  ToX Fortnite Optimizer — AMD / Intel potato texture mod (any GPU).

.DESCRIPTION
  Safely removes optional high-quality Fortnite texture packs by moving them
  to a backup folder (not permanent delete). Revert restores from backup.

  Targets ONLY:
    - *ondemand-high*   (optional HQ textures ~6GB+)
    - pakchunk100fatiad* (fat cosmetics pack, optional)

  Never touches core pakchunk*-WindowsClient base content, global.*, or Binaries.

.PARAMETER Action
  Apply | Revert

.PARAMETER PaksPath
  Optional override for ...\FortniteGame\Content\Paks
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Apply', 'Revert')]
    [string]$Action,

    [string]$PaksPath = ''
)

$ErrorActionPreference = 'Stop'
$BackupRoot = Join-Path $env:LOCALAPPDATA 'ToX Fortnite Optimizer\PotatoBackup\AmdIntel'
$ManifestPath = Join-Path $BackupRoot 'manifest.txt'

function Write-Info([string]$msg) {
    Write-Host "[ToX Potato] $msg"
}

function Find-PaksFolder {
    if ($PaksPath -and (Test-Path -LiteralPath $PaksPath)) {
        return (Resolve-Path -LiteralPath $PaksPath).Path
    }

    $candidates = @(
        'C:\Program Files\Epic Games\Fortnite\FortniteGame\Content\Paks',
        'D:\Epic Games\Fortnite\FortniteGame\Content\Paks',
        'E:\Epic Games\Fortnite\FortniteGame\Content\Paks',
        'F:\Epic Games\Fortnite\FortniteGame\Content\Paks',
        'C:\Games\Fortnite\FortniteGame\Content\Paks',
        'D:\Games\Fortnite\FortniteGame\Content\Paks'
    )

    foreach ($c in $candidates) {
        if (Test-Path -LiteralPath $c) { return $c }
    }

    # Epic launcher manifests
    $manifests = Join-Path $env:ProgramData 'Epic\EpicGamesLauncher\Data\Manifests'
    if (Test-Path -LiteralPath $manifests) {
        Get-ChildItem -LiteralPath $manifests -Filter '*.item' -ErrorAction SilentlyContinue | ForEach-Object {
            $text = Get-Content -LiteralPath $_.FullName -Raw -ErrorAction SilentlyContinue
            if ($text -match '"InstallLocation"\s*:\s*"([^"]+)"' -and $text -match 'Fortnite') {
                $install = $Matches[1] -replace '\\\\', '\'
                $paks = Join-Path $install 'FortniteGame\Content\Paks'
                if (Test-Path -LiteralPath $paks) { return $paks }
            }
        }
    }

    throw 'Fortnite Paks folder was not found. Install Fortnite or pass -PaksPath.'
}

function Get-PotatoTargets([string]$paks) {
    $files = @()
    $files += Get-ChildItem -LiteralPath $paks -File -Filter '*ondemand-high*' -ErrorAction SilentlyContinue
    $files += Get-ChildItem -LiteralPath $paks -File -Filter 'pakchunk100fatiad*' -ErrorAction SilentlyContinue
    return $files | Sort-Object FullName -Unique
}

function Test-SafeName([string]$name) {
    # Hard deny-list: never move core / global / early / non-high ondemand base chunks by accident
    if ($name -match '^(global\.|pakChunkEarly|pakchunk0-WindowsClient)') { return $false }
    if ($name -match 'ondemand-high') { return $true }
    if ($name -like 'pakchunk100fatiad*') { return $true }
    return $false
}

function Invoke-Apply([string]$paks) {
    New-Item -ItemType Directory -Force -Path $BackupRoot | Out-Null
    $targets = @(Get-PotatoTargets $paks | Where-Object { Test-SafeName $_.Name })

    if ($targets.Count -eq 0) {
        Write-Info 'No optional high texture packs found (already potato or not installed).'
        return
    }

    $moved = New-Object System.Collections.Generic.List[string]
    $bytes = 0L

    foreach ($f in $targets) {
        $dest = Join-Path $BackupRoot $f.Name
        if (Test-Path -LiteralPath $dest) {
            Remove-Item -LiteralPath $dest -Force -ErrorAction SilentlyContinue
        }

        Write-Info ("Backing up + removing: {0} ({1:N1} MB)" -f $f.Name, ($f.Length / 1MB))
        Move-Item -LiteralPath $f.FullName -Destination $dest -Force
        $moved.Add($f.Name) | Out-Null
        $bytes += $f.Length
    }

    $moved | Set-Content -LiteralPath $ManifestPath -Encoding UTF8
    Write-Info ("Done. Moved {0} files ({1:N2} GB) to backup." -f $moved.Count, ($bytes / 1GB))
    Write-Info "Backup: $BackupRoot"
}

function Invoke-Revert([string]$paks) {
    if (-not (Test-Path -LiteralPath $BackupRoot)) {
        Write-Info 'No potato backup found — nothing to revert.'
        return
    }

    $names = @()
    if (Test-Path -LiteralPath $ManifestPath) {
        $names = Get-Content -LiteralPath $ManifestPath | Where-Object { $_ -and $_.Trim() }
    }
    else {
        $names = Get-ChildItem -LiteralPath $BackupRoot -File | ForEach-Object { $_.Name }
    }

    if ($names.Count -eq 0) {
        Write-Info 'Backup folder is empty — nothing to revert.'
        return
    }

    $restored = 0
    foreach ($name in $names) {
        $src = Join-Path $BackupRoot $name
        if (-not (Test-Path -LiteralPath $src)) { continue }
        if (-not (Test-SafeName $name)) {
            Write-Info "Skipping unsafe name in backup: $name"
            continue
        }

        $dest = Join-Path $paks $name
        Write-Info "Restoring: $name"
        if (Test-Path -LiteralPath $dest) {
            Remove-Item -LiteralPath $dest -Force -ErrorAction SilentlyContinue
        }
        Move-Item -LiteralPath $src -Destination $dest -Force
        $restored++
    }

    Write-Info "Done. Restored $restored file(s)."
}

$paksFolder = Find-PaksFolder
Write-Info "Paks: $paksFolder"

if ($Action -eq 'Apply') {
    Invoke-Apply $paksFolder
}
else {
    Invoke-Revert $paksFolder
}

exit 0
