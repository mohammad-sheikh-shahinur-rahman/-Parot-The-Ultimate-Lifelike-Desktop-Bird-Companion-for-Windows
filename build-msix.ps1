# Parot - Microsoft Store MSIX Packaging Script
param(
    [string]$Version = "1.0.0.0",
    [switch]$CreateTestCert = $false
)

$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$distDir = Join-Path $projectRoot "dist-msix"
$outputMsix = Join-Path $projectRoot "Parot.msix"
$makeappx = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\makeappx.exe"

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host " Building Parot for Microsoft Store (MSIX) " -ForegroundColor Cyan
Write-Host " Identity: ITAmadersomajInc.Parot" -ForegroundColor Cyan
Write-Host " Store ID: 9NDQ2S4Q0RJT" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan

# 1. Clean and Publish Release Build
if (Test-Path $distDir) {
    Remove-Item $distDir -Recurse -Force
}
New-Item -ItemType Directory -Path $distDir -Force | Out-Null

Write-Host "Publishing Release binaries..." -ForegroundColor Yellow
& dotnet publish (Join-Path $projectRoot "Parot.csproj") -c Release -o $distDir

# 2. Copy Package Manifest and set Version
Write-Host "Configuring Package.appxmanifest (Version: $Version)..." -ForegroundColor Yellow
$manifestContent = Get-Content (Join-Path $projectRoot "Package.appxmanifest") -Raw
$manifestContent = [System.Text.RegularExpressions.Regex]::Replace($manifestContent, '(?<=<Identity\b[^>]*?\bVersion=")[^"]+', $Version)
Set-Content -Path (Join-Path $distDir "AppxManifest.xml") -Value $manifestContent -Encoding UTF8

# 3. Setup Assets for AppX
$assetsDir = Join-Path $distDir "Assets"
if (-not (Test-Path $assetsDir)) {
    New-Item -ItemType Directory -Path $assetsDir -Force | Out-Null
}

$sourceAssets = Join-Path $projectRoot "Parot-Windows-Assets"
if (Test-Path $sourceAssets) {
    Copy-Item (Join-Path $sourceAssets "*") $assetsDir -Force
    # Ensure unscaled fallbacks exist for makeappx
    if (Test-Path (Join-Path $sourceAssets "Square150x150Logo.scale-200.png")) {
        Copy-Item (Join-Path $sourceAssets "Square150x150Logo.scale-200.png") (Join-Path $assetsDir "Square150x150Logo.png") -Force
    }
    if (Test-Path (Join-Path $sourceAssets "Square44x44Logo.scale-200.png")) {
        Copy-Item (Join-Path $sourceAssets "Square44x44Logo.scale-200.png") (Join-Path $assetsDir "Square44x44Logo.png") -Force
    }
    if (Test-Path (Join-Path $sourceAssets "Wide310x150Logo.scale-200.png")) {
        Copy-Item (Join-Path $sourceAssets "Wide310x150Logo.scale-200.png") (Join-Path $assetsDir "Wide310x150Logo.png") -Force
    }
    if (Test-Path (Join-Path $sourceAssets "SplashScreen.scale-200.png")) {
        Copy-Item (Join-Path $sourceAssets "SplashScreen.scale-200.png") (Join-Path $assetsDir "SplashScreen.png") -Force
    }
}

# 4. Create MSIX Package
Write-Host "Packaging MSIX with MakeAppx..." -ForegroundColor Yellow
if (Test-Path $outputMsix) {
    Remove-Item $outputMsix -Force
}

& $makeappx pack /d $distDir /p $outputMsix /o /v

if (Test-Path $outputMsix) {
    Write-Host "`nSUCCESS! Package created at: $outputMsix" -ForegroundColor Green
    Write-Host "Ready for upload to Microsoft Partner Center!" -ForegroundColor Green
} else {
    Write-Error "Failed to generate MSIX package."
}
