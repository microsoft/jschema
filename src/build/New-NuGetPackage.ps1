<#
.SYNOPSIS
Create a NuGet package whose version is specified by the information in the CurrentVersion.xml
file.
#>

param(
    [Parameter(Mandatory=$true)] $packageName,
    [Parameter(Mandatory=$true)] $binariesDirectory,
    [Parameter(Mandatory=$true)] $configuration
)

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."
$outputDirectory = "$binariesDirectory..\..\NuGet\$configuration"

if (-not (Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$outputDirectory = Resolve-Path "$outputDirectory"

"Creating NuGet package file $outputDirectory\$packageName.nupkg..."

$major, $minor, $patch, $preRelease = & "$PSScriptRoot\Get-VersionConstants.ps1"

& "$repoRoot\.nuget\NuGet.exe" pack "$packageName.nuspec" `
 -Verbosity Quiet `
 -Properties id=Microsoft.$packageName`;major=$major`;minor=$minor`;patch=$patch`;prerelease=$prerelease `
 -BasePath $binariesDirectory `
 -OutputDirectory $outputDirectory

if ($?) {
   "NuGet package file creation succeeded."
} else {
   "ERROR: Failed to create package file"
}