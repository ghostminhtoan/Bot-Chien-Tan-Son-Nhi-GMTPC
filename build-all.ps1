# ==============================================================================
# Bot Chien Tan Son Nhi POS - Multi-Platform Master Build Script
# Target: Windows Standalone EXE, Linux Binary + Tar.gz + Deb, Android Signed APK
# ==============================================================================

$ErrorActionPreference = "Stop"
Write-Host "==========================================================" -ForegroundColor Yellow
Write-Host " BOT CHIEN TAN SON NHI POS - MASTER MULTI-PLATFORM BUILD" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Yellow

$publishDir = Join-Path $PSScriptRoot "publish"
$artifactsDir = Join-Path $PSScriptRoot "artifacts"

if (-not (Test-Path $publishDir)) { New-Item -ItemType Directory -Path $publishDir -Force | Out-Null }
if (-not (Test-Path $artifactsDir)) { New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null }

# 1. BUILD WINDOWS (Single-File Standalone EXE)
Write-Host "`n[1/3] Xuat ban Windows x64 Single-file Standalone EXE..." -ForegroundColor Green
dotnet publish "$PSScriptRoot/BotChienTanSonNhi/BotChienTanSonNhi.Desktop/BotChienTanSonNhi.Desktop.csproj" `
    -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "$artifactsDir/win-x64"

Copy-Item "$artifactsDir/win-x64/BotChienTanSonNhi.Desktop.exe" "$publishDir/BotChienTanSonNhi-GMTPC.Desktop.exe" -Force
Write-Host "  -> Da tao: $publishDir/BotChienTanSonNhi-GMTPC.Desktop.exe" -ForegroundColor Gray

# 2. BUILD LINUX (Standalone Binary, Portable Tar.gz, Debian Package .deb)
Write-Host "`n[2/3] Xuat ban Linux x64 (Binary, Tar.gz, Deb)..." -ForegroundColor Green
dotnet publish "$PSScriptRoot/BotChienTanSonNhi/BotChienTanSonNhi.Desktop/BotChienTanSonNhi.Desktop.csproj" `
    -c Release -r linux-x64 --self-contained true `
    -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true `
    -o "$artifactsDir/linux-x64"

# 2a. Standalone binary
Copy-Item "$artifactsDir/linux-x64/BotChienTanSonNhi.Desktop" "$publishDir/BotChienTanSonNhi-GMTPC" -Force
Write-Host "  -> Da tao: $publishDir/BotChienTanSonNhi-GMTPC" -ForegroundColor Gray

# 2b. Portable tar.gz
Push-Location $publishDir
tar -czf "BotChienTanSonNhi-GMTPC-linux-x64.tar.gz" "BotChienTanSonNhi-GMTPC"
Pop-Location
Write-Host "  -> Da tao: $publishDir/BotChienTanSonNhi-GMTPC-linux-x64.tar.gz" -ForegroundColor Gray

# 2c. Debian package (.deb)
$debDir = "$artifactsDir/deb_build"
if (Test-Path $debDir) { Remove-Item -Recurse -Force $debDir }
New-Item -ItemType Directory -Path "$debDir/DEBIAN" -Force | Out-Null
New-Item -ItemType Directory -Path "$debDir/usr/bin" -Force | Out-Null
New-Item -ItemType Directory -Path "$debDir/usr/share/applications" -Force | Out-Null

Copy-Item "$publishDir/BotChienTanSonNhi-GMTPC" "$debDir/usr/bin/BotChienTanSonNhi-GMTPC" -Force

$control = @"
Package: botchientansonnhi-gmtpc
Version: 1.0.0
Section: utils
Priority: optional
Architecture: amd64
Maintainer: GMTPC <admin@gmtpc.local>
Description: Bot Chien Tan Son Nhi POS System
 He thong ban hang Bot Chien & Nui Chien Tan Son Nhi tren Linux Avalonia.
"@
[System.IO.File]::WriteAllText("$debDir/DEBIAN/control", ($control.Replace("`r`n", "`n") + "`n"), (New-Object System.Text.UTF8Encoding($false)))

$desktop = @"
[Desktop Entry]
Name=Bot Chien Tan Son Nhi POS
Comment=He thong ban hang POS
Exec=/usr/bin/BotChienTanSonNhi-GMTPC
Terminal=false
Type=Application
Categories=Utility;Office;
"@
[System.IO.File]::WriteAllText("$debDir/usr/share/applications/BotChienTanSonNhi-GMTPC.desktop", ($desktop.Replace("`r`n", "`n") + "`n"), (New-Object System.Text.UTF8Encoding($false)))
[System.IO.File]::WriteAllText("$debDir/debian-binary", "2.0`n", (New-Object System.Text.UTF8Encoding($false)))

Push-Location $debDir
tar -czf control.tar.gz -C DEBIAN control
tar -czf data.tar.gz usr
Pop-Location

function New-ArArchiveInternal {
    param([string]$OutputFile, [string[]]$InputFiles)
    $fs = [System.IO.File]::Create($OutputFile)
    $magic = [System.Text.Encoding]::ASCII.GetBytes("!<arch>`n")
    $fs.Write($magic, 0, $magic.Length)

    foreach ($file in $InputFiles) {
        $fi = New-Object System.IO.FileInfo($file)
        $bytes = [System.IO.File]::ReadAllBytes($file)
        $name = ($fi.Name + "/").PadRight(16, ' ')
        $mtime = "0".PadRight(12, ' ')
        $owner = "0".PadRight(6, ' ')
        $group = "0".PadRight(6, ' ')
        $mode = "100644  "
        $size = ($bytes.Length.ToString()).PadRight(10, ' ')
        $trailer = [byte[]]@(0x60, 0x0A)

        $headerStr = "$name$mtime$owner$group$mode$size"
        $headerBytes = [System.Text.Encoding]::ASCII.GetBytes($headerStr)
        $fs.Write($headerBytes, 0, $headerBytes.Length)
        $fs.Write($trailer, 0, $trailer.Length)
        $fs.Write($bytes, 0, $bytes.Length)

        if ($bytes.Length % 2 -ne 0) {
            $fs.WriteByte(0x0A)
        }
    }
    $fs.Close()
}

New-ArArchiveInternal -OutputFile "$publishDir/BotChienTanSonNhi-GMTPC_1.0.0_amd64.deb" -InputFiles @("$debDir/debian-binary", "$debDir/control.tar.gz", "$debDir/data.tar.gz")
Write-Host "  -> Da tao: $publishDir/BotChienTanSonNhi-GMTPC_1.0.0_amd64.deb" -ForegroundColor Gray

# 3. BUILD ANDROID (Signed Standalone APK)
Write-Host "`n[3/3] Xuat ban Android Signed Standalone APK..." -ForegroundColor Green
dotnet publish "$PSScriptRoot/BotChienTanSonNhi/BotChienTanSonNhi.Android/BotChienTanSonNhi.Android.csproj" `
    -c Release -f net10.0-android -p:AndroidPackageFormat=apk `
    -o "$artifactsDir/android"

$signedApk = Get-ChildItem "$artifactsDir/android" -Filter "*Signed.apk" | Select-Object -First 1
if ($signedApk) {
    Copy-Item $signedApk.FullName "$publishDir/com.CompanyName.BotChienTanSonNhi-GMTPC-Signed.apk" -Force
    Write-Host "  -> Da tao: $publishDir/com.CompanyName.BotChienTanSonNhi-GMTPC-Signed.apk" -ForegroundColor Gray
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " HOAN TAT XUAT XUONG TOAN BO GOI PUBLISH (0 ERROR)" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
Get-ChildItem $publishDir | Select-Object Name, Length, LastWriteTime | Format-Table -AutoSize
