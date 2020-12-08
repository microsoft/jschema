<#
.SYNOPSIS
    Build, test, and package JSchema.
.DESCRIPTION
    Builds JSchema for multiple target frameworks, runs the tests, and creates
    NuGet packages.
.PARAMETER Configuration
    The build configuration. Default=Release
.PARAMETER RunJsonSchemaTestSuite
    Run the official JSON Schema test suite.
.PARAMETER NoBuild
    Do not build.
.PARAMETER NoTest
    Do not run tests.
.PARAMETER NoPackage
    Do not create NuGet packages.
#>

[CmdletBinding()]
param(
   [string]
   [ValidateSet("Debug", "Release")]
   $Configuration="Release",
   
   [switch]
   $RunJsonSchemaTestSuite,

   [switch]
   $NoBuild,

   [switch]
   $NoTest,

   [switch]
   $NoPackage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

$SourceRoot = "$PSScriptRoot\src"
$SolutionFile = "$SourceRoot\Everything.sln"
$Platform = "AnyCPU"
$BuildDirectory = "$PSScriptRoot\bld"
$PackageOutputDirectory = "$BuildDirectory\bin\NuGet\$Configuration"

function Exit-WithFailureMessage($message) {
    Write-Information $message
    Write-Information "SCRIPT FAILED."
    exit 1
}

function Get-PackageLicenseExpression() {
    $buildPropsPath = "$PSScriptRoot\src\build.props"
    $namespace = @{ msbuild = "http://schemas.microsoft.com/developer/msbuild/2003" }
    $xPath = "/msbuild:Project/msbuild:PropertyGroup[@Label='Package']/msbuild:PackageLicenseExpression"
    $xml = Select-Xml -Path $buildPropsPath -Namespace $namespace -XPath $xPath
    $packageLicenseExpression = $xml.Node.InnerText

    $packageLicenseExpression
}

function Invoke-Build {
    if (Test-Path $buildDirectory) {
        Remove-Item -Force -Recurse $buildDirectory
    }

    dotnet build --no-incremental --configuration $Configuration /fileloggerparameters:Verbosity=detailed $SolutionFile
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage "Build failed."
    }
}

function Invoke-Tests($project) {
    dotnet test --no-build --no-restore --configuration $Configuration src\$project.UnitTests\$project.UnitTests.csproj
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage "$project unit tests failed."
    }
}

# Publish-Application
#
# This function invokes "dotnet publish" to create a "publish" directory
# containing all files necessary to execute an application. In particular,
# dotnet publish adds the binaries from any dependent NuGet packages to the
# publish directory.
#
# This operation is necessary only when building an application for .NET Core.
# When building for .NET Framework, the build output directory already contains
# the necessary dependencies.
function Publish-Application($project, $framework) {
    Write-Information "Publishing $project for $framework ..."
    dotnet publish $SourceRoot\$project\$project.csproj --no-restore --configuration $Configuration --framework $framework
}

function New-NuGetPackageFromProjectFile($project, $version) {
    $projectFile = "$PSScriptRoot\src\$project\$project.csproj"
    
    $arguments =
        "pack", $projectFile,
        "--configuration", $Configuration,
        "--no-build", "--no-restore",
        "--include-source", "--include-symbols",
        "-p:Platform=$Platform",
        "--output", $PackageOutputDirectory

    Write-Debug "dotnet $($arguments -join ' ')"

    dotnet $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage "$project NuGet package creation failed."
    }
}

function New-NuGetPackageFromNuspecFile($project, $version, $packageLicenseExpression, $suffix = "") {
    $nuspecFile = "$PSScriptRoot\src\$project\$project.nuspec"

    $arguments=
        "pack", $nuspecFile,
        "-Symbols",
        "-Properties", "platform=$Platform;configuration=$Configuration;version=$version;packageLicenseExpression=$packageLicenseExpression",
        "-Verbosity", "Quiet",
        "-BasePath", ".\",
        "-OutputDirectory", $PackageOutputDirectory

    if ($suffix -ne "") {
        $arguments += "-Suffix", $Suffix
    }

    $nugetExePath = "$PSScriptRoot\.nuget\NuGet.exe"

    Write-Debug "$nuGetExePath $($arguments -join ' ')"

    &$nuGetExePath $arguments
    if ($LASTEXITCODE -ne 0) {
        Exit-WithFailureMessage "$project NuGet package creation failed."
    } else {
        Write-Information "Successfully created package from $nuspecFile"
    }
}

if (-not $NoBuild) {
    Invoke-Build
}

if ($RunJsonSchemaTestSuite) {
    # To understand the reason for this, see the comment in
    # src\Json.Schema.Validation.UnitTests\ValidationSuite.cs.
    $jsonSchemaTestSuitePath = "$PSScriptRoot\..\JSON-Schema-Test-Suite"
    if (-not (Test-Path $jsonSchemaTestSuitePath)) {
        $jsonSchemaTestSuiteUri = "https://github.com/json-schema-org/JSON-Schema-Test-Suite"
        Write-Information "Cloning the offical JSON schema test suite..."
        git clone $jsonSchemaTestSuiteUri $jsonSchemaTestSuitePath
    }
}

if (-not $NoTest) {
    $testedProjects = "Json.Pointer", "Json.Schema", "Json.Schema.ToDotNet", "Json.Schema.Validation"
    foreach ($project in $testedProjects) {
        Invoke-Tests $project
    }
}

if (-not $NoPackage) {
    $versionPrefix, $versionSuffix = .\src\build\Get-VersionConstants.ps1
    $version = "$versionPrefix$versionSuffix"

    $packagingProjects = "Json.Pointer", "Json.Schema"
    foreach ($project in $packagingProjects) {
        New-NuGetPackageFromProjectFile $project $version
    }

    $nuspecProjects = "Json.Schema.ToDotNet.Cli", "Json.Schema.Validation.Cli"
    foreach ($project in $nuspecProjects) {
        Publish-Application $project netcoreapp2.1
        New-NuGetPackageFromNuSpecFile $project $version $(Get-PackageLicenseExpression)
    }
}