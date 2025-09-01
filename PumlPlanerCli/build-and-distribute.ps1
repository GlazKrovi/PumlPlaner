#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and distribute PumlPlaner CLI tool
    
.DESCRIPTION
    This script builds, packs, and distributes the PumlPlaner CLI tool
    as a global .NET tool.
    
.PARAMETER Version
    Version to build (default: auto-increment patch)
    
.PARAMETER Configuration
    Build configuration (default: Release)
    
.PARAMETER PushToNuGet
    Push to NuGet after building
    
.EXAMPLE
    .\build-and-distribute.ps1
    .\build-and-distribute.ps1 -Version 1.1.0 -PushToNuGet
#>

param(
    [string]$Version = "",
    [string]$Configuration = "Release",
    [switch]$PushToNuGet
)

# Colors for output
$Green = "`e[32m"
$Yellow = "`e[33m"
$Red = "`e[31m"
$Reset = "`e[0m"

function Write-Info { param($Message) Write-Host "$Green[INFO]$Reset $Message" }
function Write-Warning { param($Message) Write-Host "$Yellow[WARN]$Reset $Message" }
function Write-Error { param($Message) Write-Host "$Red[ERROR]$Reset $Message" }

try {
    Write-Info "Starting PumlPlaner CLI build and distribution..."
    
    # Clean previous builds
    Write-Info "Cleaning previous builds..."
    dotnet clean --configuration $Configuration
    if ($LASTEXITCODE -ne 0) { throw "Clean failed" }
    
    # Restore packages
    Write-Info "Restoring packages..."
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Restore failed" }
    
    # Build project
    Write-Info "Building project..."
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }
    
    # Run tests if they exist
    if (Test-Path "../PumlPlanerTester") {
        Write-Info "Running tests..."
        dotnet test "../PumlPlanerTester" --configuration $Configuration --no-build
        if ($LASTEXITCODE -ne 0) { 
            Write-Warning "Tests failed, but continuing with build..."
        }
    }
    
    # Pack as tool
    Write-Info "Packing as global tool..."
    dotnet pack --configuration $Configuration --no-build --output ./nupkg
    if ($LASTEXITCODE -ne 0) { throw "Pack failed" }
    
    # Get the generated package
    $nupkgFile = Get-ChildItem "./nupkg/*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $nupkgFile) { throw "No .nupkg file found" }
    
    Write-Info "Generated package: $($nupkgFile.Name)"
    
    # Test local installation
    Write-Info "Testing local installation..."
    dotnet tool uninstall -g PumlPlanerCli 2>$null
    dotnet tool install -g --add-source ./nupkg PumlPlanerCli
    if ($LASTEXITCODE -ne 0) { throw "Local installation test failed" }
    
    # Test the tool
    $toolOutput = pumlplaner --help 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Tool execution test failed" }
    
    Write-Info "Local installation test successful!"
    
    # Uninstall test version
    dotnet tool uninstall -g PumlPlanerCli
    
    if ($PushToNuGet) {
        Write-Info "Pushing to NuGet..."
        
        # Check if API key is set
        $apiKey = $env:NUGET_API_KEY
        if (-not $apiKey) {
            $apiKey = Read-Host "Enter your NuGet API key"
        }
        
        dotnet nuget push $nupkgFile.FullName --api-key $apiKey --source nuget.org
        if ($LASTEXITCODE -ne 0) { throw "NuGet push failed" }
        
        Write-Info "Successfully pushed to NuGet!"
        Write-Info "Users can now install with: dotnet tool install -g PumlPlanerCli"
    } else {
        Write-Info "Build completed successfully!"
        Write-Info "Package location: $($nupkgFile.FullName)"
        Write-Info "To push to NuGet, run: .\build-and-distribute.ps1 -PushToNuGet"
    }
    
    # Show distribution instructions
    Write-Info "`n📦 Distribution Summary:"
    Write-Info "  • Package: $($nupkgFile.Name)"
    Write-Info "  • Size: $([math]::Round($nupkgFile.Length / 1KB, 2)) KB"
    Write-Info "  • Location: $($nupkgFile.FullName)"
    
    if (-not $PushToNuGet) {
        Write-Info "`n🚀 Next steps:"
        Write-Info "  1. Test the package locally"
        Write-Info "  2. Run: .\build-and-distribute.ps1 -PushToNuGet"
        Write-Info "  3. Or distribute manually: $($nupkgFile.FullName)"
    }
    
} catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    exit 1
}
