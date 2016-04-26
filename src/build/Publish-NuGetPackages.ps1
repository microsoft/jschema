<#
.SYNOPSIS
Publish-NuGetPackages.ps1

.DESCRIPTION
Publish the JSchema NuGet packages and immediately delist them.
#>

param(
    [Parameter(Mandatory=$true)] $ApiKey,
    [Parameter(Mandatory=$false)] $PackageSource = "https://nuget.org"
)

$packagesDirectory = "$PSScriptRoot\..\..\bld\bin\NuGet\Release"
if (-not (Test-Path $packagesDirectory)) {
    Write-Host -ForegroundColor Red "Error: The NuGet output directory '$packagesDirectory' does not exist. Have you built?"
    exit 1;
}

$packagesDirectory = Resolve-Path $packagesDirectory

$nugetDirectory = Resolve-Path "$PSScriptRoot\..\..\.nuget"
$nugetExe = "$nugetDirectory\nuget.exe"

$major, $minor, $patch, $preRelease = & "$PSScriptRoot\Get-VersionConstants.ps1"

$packageVersion = "$major.$minor.$patch$preRelease"

$packages = "Microsoft.Json.Schema", "Microsoft.Json.Schema.ToDotNet"

$packages | ForEach-Object {
    $nupkg = "$_.$packageVersion.nupkg"
    & $nugetExe push $packagesDirectory\$nupkg $ApiKey -Source $PackageSource
    if (!$?) {
        Write-Host -ForegroundColor Red "Error: failed to push $nupkg to $PackageSource"
        exit 1;
    }
    & $nugetExe delete $_ $packageVersion $ApiKey -Source $PackageSource
    if (!$?) {
        Write-Host -ForegroundColor Red "Error: failed to delist $nupkg from $PackageSource"
        exit 1;
    }
}