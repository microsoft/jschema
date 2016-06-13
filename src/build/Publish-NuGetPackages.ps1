<#
.SYNOPSIS
Publish-NuGetPackages.ps1

.DESCRIPTION
Publish the JSchema NuGet packages and immediately delist them.
#>

param(
    [Parameter(Mandatory=$false)] $PackageSource = "https://nuget.org"
)

function Get-ApiKey {
    $apiKeyFilePath = Resolve-Path "$PSScriptRoot\..\..\..\SetNuGetJSchemaApiKey.cmd"
    $apiSettingCommand = Get-Content $apiKeyFilePath
    $apiSettingCommand -match "set API_KEY=`"([^`"]+)`"" | Out-Null
    $Matches[1]
}

function Prompt-Publish($packageName) {
    $title = "Publish Package $packageName"
    $message = "OK to publish ${packageName}?"

    $yes = New-Object System.Management.Automation.Host.ChoiceDescription "&Yes", "Publish"
    $no = New-Object System.Management.Automation.Host.ChoiceDescription "&No", "Do not publish"

    $options = [System.Management.Automation.Host.ChoiceDescription[]] ($yes, $no)

    $Host.UI.PromptForChoice($title, $message, $options, 0)
}

function Publish-Package($packageName, $packageVersion, $apiKey) {
    $result = Prompt-Publish $packageName
    if ($result -eq 0) {
        $nupkg = "$packageName.$packageVersion.nupkg"
        & $nugetExe push $packagesDirectory\$nupkg $apiKey -Source $PackageSource
        if (!$?) {
            Write-Host -ForegroundColor Red "Error: failed to push $nupkg to $PackageSource"
            exit 1;
        }
        & $nugetExe delete $packageName $packageVersion $ApiKey -Source $PackageSource
        if (!$?) {
            Write-Host -ForegroundColor Red "Error: failed to delist $nupkg from $PackageSource"
            exit 1;
        }
    } else {
        Write-Host -ForegroundColor Yellow "Package $packageName was not published."
    }
}

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
$apiKey = Get-ApiKey

$packages = "Microsoft.Json.Pointer", "Microsoft.Json.Schema", "Microsoft.Json.Schema.ToDotNet"

$packages | ForEach-Object { Publish-Package $_ $packageVersion $apiKey }
