@echo off
setlocal

if exist bld rmdir /s /q bld

set Configuration=Release

REM Set environment variables containing the components of the versions for the
REM .NET assemblies and corresponding NuGet packages.
call SetCurrentVersion.cmd

REM Restore NuGet packages.
.nuget\NuGet.exe restore src\Everything.sln -ConfigFile .nuget\NuGet.Config

REM Build solution.
msbuild /verbosity:minimal /target:rebuild src\Everything.sln /p:Configuration=%Configuration%
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

REM Run tests.
set XUNIT=src\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe

%XUNIT% bld\bin\Json.Schema.ToDotNet.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.ToDotNet.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

%XUNIT% bld\bin\Json.Schema.UnitTests\AnyCPU_%Configuration%\Microsoft.Json.Schema.UnitTests.dll
if "%ERRORLEVEL%" NEQ "0" (
goto ExitFailed
)

REM Build NuGet packages.
set NUGET_OUTPUT_DIRECTORY= bld\bin\NuGet

if not exist %NUGET_OUTPUT_DIRECTORY% mkdir %NUGET_OUTPUT_DIRECTORY%

.nuget\NuGet.exe pack src\Json.Schema\Json.Schema.nuspec ^
 -Properties id=Microsoft.Json.Schema;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% ^
 -Verbosity Quiet ^
 -BasePath bld\bin\Json.Schema\AnyCPU_Release ^
 -OutputDirectory %NUGET_OUTPUT_DIRECTORY%

.nuget\NuGet.exe pack src\Json.Schema.ToDotNet\Json.Schema.ToDotNet.nuspec ^
 -Properties id=Microsoft.Json.Schema.ToDotNet;major=%MAJOR%;minor=%MINOR%;patch=%PATCH%;prerelease=%PRERELEASE% ^
 -Verbosity Quiet ^
 -BasePath bld\bin\Json.Schema.ToDotNet\AnyCPU_Release ^
 -OutputDirectory %NUGET_OUTPUT_DIRECTORY%

goto Exit

:ExitFailed
@echo .
@echo SCRIPT FAILED

:Exit

endlocal && exit /b %ERRORLEVEL%