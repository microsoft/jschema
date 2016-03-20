<#
.SYNOPSIS
Read the contents of CurrentVersion.xml and return the individual version number components.
#>

$currentVersionXmlFile = "$PSScriptRoot\CurrentVersion.xml"
[xml]$currentVersionInfo = Get-Content $currentVersionXmlFile

$currentVersion = $currentVersionInfo.CurrentVersion
$currentVersion.Major, $currentVersion.Minor, $currentVersion.Patch, $currentVersion.PreRelease
