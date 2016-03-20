<#
.SYNOPSIS
Uses the information in the file CurrentVersion.xml to synthesize a file containing
compilation constants used to set the version attributes of the assembly being built.
#>

param(
    [Parameter(Mandatory=$true)] $outputPath,
    [Parameter(Mandatory=$true)] $namespace
)

$currentVersionXmlFile = "$PSScriptRoot\CurrentVersion.xml"
[xml]$currentVersionInfo = Get-Content $currentVersionXmlFile

$currentVersion = $currentVersionInfo.CurrentVersion
$major = $currentVersion.Major
$minor = $currentVersion.Minor
$patch = $currentVersion.Patch
$preRelease = $currentVersion.PreRelease

$versionConstantsFileContents =
@"
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace $namespace
{
    public static class VersionConstants
    {
        public const string PreRelease = "$preRelease";
        public const string AssemblyVersion = "$major.$minor.$patch";
        public const string FileVersion = AssemblyVersion + ".0";
        public const string Version = AssemblyVersion + PreRelease;
    }
}
"@

$outputFile = "$outputPath\VersionConstants.cs"

Set-Content $outputFile $versionConstantsFileContents
