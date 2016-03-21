<#
.SYNOPSIS
Create a NuGet package whose version is specified by the information in the CurrentVersion.xml
file.
#>

param(
    [Parameter(Mandatory=$true)] $projectName,
    [Parameter(Mandatory=$true)] $configuration,
    [Parameter(Mandatory=$true)] $outputDirectory
)

$outputDirectory = "$outputDirectory\$configuration"

if (-not (Test-Path $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}

$outputDirectory = Resolve-Path "$outputDirectory"

"Creating NuGet package file $outputDirectory\$projectName.nupkg..."

$repoRoot = Resolve-Path "$PSScriptRoot\..\.."

$major, $minor, $patch, $preRelease = & "$PSScriptRoot\Get-VersionConstants.ps1"

& "$repoRoot\.nuget\NuGet.exe" pack "$projectName.nuspec" `
 -Verbosity Quiet `
 -Properties id=Microsoft.$projectName`;major=$major`;minor=$minor`;patch=$patch`;prerelease=$prerelease `
 -BasePath $repoRoot\bld\bin\$projectName\AnyCPU_$configuration `
 -OutputDirectory $outputDirectory

if ($?) {
   "NuGet package file creation succeeded."
} else {
   "ERROR: Failed to create package file"
}