# Publish all CLI and Service builds for multiple runtimes
# Requires: .NET 8 SDK
# Usage: powershell -ExecutionPolicy Bypass -File scripts/publish-all.ps1

$ErrorActionPreference = "Stop"

# RIDs to build
$cliRids = @(
  "win-x64",
  "osx-x64",
  "osx-arm64",
  "linux-x64",
  "linux-arm64",
  "linux-musl-x64",
  "linux-musl-arm64"
)

$serviceRids = @(
  "win-x64",
  "win-arm64",
  "osx-x64",
  "osx-arm64",
  "linux-x64",
  "linux-arm64",
  "linux-musl-x64",
  "linux-musl-arm64"
)

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
# Move to repo root (script may be invoked from elsewhere)
Set-Location (Resolve-Path "$root\..")

$artifactsRoot = "artifacts"
New-Item -ItemType Directory -Force -Path $artifactsRoot | Out-Null

function Publish-Project {
  param(
    [Parameter(Mandatory=$true)][string]$Project,
    [Parameter(Mandatory=$true)][string]$Rid,
    [Parameter(Mandatory=$true)][string]$OutDir
  )

  New-Item -ItemType Directory -Force -Path $OutDir | Out-Null

  Write-Host "Publishing $Project for $Rid -> $OutDir"
  dotnet publish $Project `
    -c Release `
    -r $Rid `
    --self-contained true `
    -p:PublishSingleFile=true `
    -o $OutDir
}

function Zip-Directory {
  param(
    [Parameter(Mandatory=$true)][string]$SourceDir,
    [Parameter(Mandatory=$true)][string]$ZipPath
  )

  if (Test-Path $ZipPath) { Remove-Item $ZipPath -Force }
  Write-Host "Zipping $SourceDir -> $ZipPath"
  Add-Type -AssemblyName System.IO.Compression.FileSystem
  [System.IO.Compression.ZipFile]::CreateFromDirectory($SourceDir, $ZipPath)
}

# Build CLI
foreach ($rid in $cliRids) {
  $outDir = Join-Path $artifactsRoot "cli\$rid"
  Publish-Project -Project "SemanticSlicer.Cli/SemanticSlicer.Cli.csproj" -Rid $rid -OutDir $outDir

  $zipName = "SemanticSlicer.Cli-$rid.zip"
  $zipPath = Join-Path $artifactsRoot $zipName
  Zip-Directory -SourceDir $outDir -ZipPath $zipPath
}

# Build Service
foreach ($rid in $serviceRids) {
  $outDir = Join-Path $artifactsRoot "service\$rid"
  Publish-Project -Project "SemanticSlicer.Service/SemanticSlicer.Service.csproj" -Rid $rid -OutDir $outDir

  $zipName = "SemanticSlicer.Service-$rid.zip"
  $zipPath = Join-Path $artifactsRoot $zipName
  Zip-Directory -SourceDir $outDir -ZipPath $zipPath
}

Write-Host "All builds complete. Zips are in ./artifacts"
# NuGet pack/push (library)
# Pack the SemanticSlicer library .nupkg and place it under ./artifacts/nuget
$nugetOut = Join-Path $artifactsRoot "nuget"
New-Item -ItemType Directory -Force -Path $nugetOut | Out-Null

Write-Host "Packing NuGet for SemanticSlicer"
dotnet pack "SemanticSlicer/SemanticSlicer.csproj" -c Release -o $nugetOut

# Optional: push to NuGet (requires API key). Uncomment and set $env:NUGET_API_KEY
# Write-Host "Pushing package to NuGet.org"
# dotnet nuget push (Join-Path $nugetOut "*.nupkg") --source "https://api.nuget.org/v3/index.json" --api-key $env:NUGET_API_KEY