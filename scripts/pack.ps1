$ErrorActionPreference = "Stop"

$version = $env:APPVEYOR_BUILD_VERSION
if ($env:APPVEYOR_REPO_BRANCH -eq 'develop') {
	$version = $version -replace "develop","beta"
}
if ($env:APPVEYOR_REPO_TAG_NAME) {
	$version = $env:APPVEYOR_REPO_TAG_NAME
}
Write-Host "Packing version: $version"

$buildFolder = (Get-Item $PSScriptRoot).Parent.FullName
if($env:APPVEYOR_BUILD_FOLDER) {
	$buildFolder = $env:APPVEYOR_BUILD_FOLDER
}
Write-Host "Working from build folder: $buildFolder"

$projectPaths = Join-Path $buildFolder 'src\Cabinet*'
$outPath = Join-Path $buildFolder "artifacts"

Get-ChildItem $projectPaths | Foreach { 
    dotnet pack $_ `
        --output $outPath `
        --configuration Release `
        /p:PackageVersion=$version
}