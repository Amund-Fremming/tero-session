#!/usr/bin/env pwsh
# Clean build script to prevent recursive path issues

Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
dotnet clean

Write-Host "Removing bin/obj directories..." -ForegroundColor Yellow
Remove-Item -Recurse -Force .\bin, .\obj, .\tests\bin, .\tests\obj -ErrorAction SilentlyContinue

Write-Host "Building solution..." -ForegroundColor Green
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit $LASTEXITCODE
}
