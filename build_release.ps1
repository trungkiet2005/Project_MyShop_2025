# Build and Publish Script for MyShop 2025
# This script builds the application and prepares it for installation

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "  MyShop 2025 - Build & Publish Script" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "Project_MyShop_2025"
$releaseDir = "Release"
$portableDir = "$releaseDir\Portable"
$installerDir = "$releaseDir\Installer"

# Step 1: Create Release directories
Write-Host "[1/4] Creating Release directories..." -ForegroundColor Yellow
if (-not (Test-Path $releaseDir)) {
    New-Item -ItemType Directory -Path $releaseDir | Out-Null
}
if (-not (Test-Path $portableDir)) {
    New-Item -ItemType Directory -Path $portableDir | Out-Null
}
if (-not (Test-Path $installerDir)) {
    New-Item -ItemType Directory -Path $installerDir | Out-Null
}

# Step 2: Build the application in Debug mode (safer, no trimming issues)
Write-Host "[2/4] Building application in Debug mode..." -ForegroundColor Yellow
dotnet build $projectPath --configuration Debug --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed!" -ForegroundColor Red
    exit 1
}

# Step 3: Find the build output and copy to Portable folder
Write-Host "[3/4] Copying build output to Portable folder..." -ForegroundColor Yellow
$buildOutput = "$projectPath\bin\Debug\net9.0-windows10.0.19041.0\win-x64"
$appxOutput = "$projectPath\bin\Debug\net9.0-windows10.0.19041.0\win-x64\AppX"

# Clean portable folder
if (Test-Path $portableDir) {
    Remove-Item -Path "$portableDir\*" -Recurse -Force -ErrorAction SilentlyContinue
}

# Try to copy from AppX first (packaged output), fallback to main output
if (Test-Path $appxOutput) {
    Copy-Item -Path "$appxOutput\*" -Destination $portableDir -Recurse -Force
    Write-Host "   Copied from AppX folder" -ForegroundColor Gray
}
elseif (Test-Path $buildOutput) {
    Copy-Item -Path "$buildOutput\*" -Destination $portableDir -Recurse -Force
    Write-Host "   Copied from build output folder" -ForegroundColor Gray
}
else {
    # Try simpler path
    $simplePath = "$projectPath\bin\Debug\net9.0-windows10.0.19041.0"
    if (Test-Path $simplePath) {
        Copy-Item -Path "$simplePath\*" -Destination $portableDir -Recurse -Force
        Write-Host "   Copied from $simplePath" -ForegroundColor Gray
    }
    else {
        Write-Host "WARNING: Could not find build output. You may need to copy files manually." -ForegroundColor Yellow
    }
}

# Step 4: Display completion message
Write-Host ""
Write-Host "============================================" -ForegroundColor Green
Write-Host "  BUILD COMPLETED SUCCESSFULLY!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Portable files location:" -ForegroundColor Cyan
Write-Host "  $portableDir" -ForegroundColor White
Write-Host ""
Write-Host "Next steps to create installer:" -ForegroundColor Yellow
Write-Host "  1. Download Inno Setup from: https://jrsoftware.org/isdl.php" -ForegroundColor White
Write-Host "  2. Install Inno Setup (free, ~3MB)" -ForegroundColor White
Write-Host "  3. Open 'installer.iss' with Inno Setup Compiler" -ForegroundColor White
Write-Host "  4. Press Ctrl+F9 or click 'Compile' button" -ForegroundColor White
Write-Host "  5. Installer will be created at: $installerDir\MyShop2025_Setup_1.0.0.exe" -ForegroundColor White
Write-Host ""
Write-Host "Alternatively, you can use the Portable folder directly:" -ForegroundColor Yellow
Write-Host "  - Just copy the entire '$portableDir' folder to any computer" -ForegroundColor White
Write-Host "  - Run 'Project_MyShop_2025.exe' to start the application" -ForegroundColor White
Write-Host ""
